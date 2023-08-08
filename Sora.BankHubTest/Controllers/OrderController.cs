using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sora.BankHubTest.Caches;
using Sora.BankHubTest.Dtos;
using Sora.BankHubTest.Entities;
using Sora.BankHubTest.Https;
using Sora.BankHubTest.Https.Dtos;
using Sora.BankHubTest.Repositories;
using System.Net;

namespace Sora.BankHubTest.Controllers
{
    [Route("")]
    public class OrderController : BankHubTestBaseController
    {
        #region Khởi tạo

        private readonly IBankAccountRepository _bankAccountRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOptions<JPBankHubOption> _jpBankHubOptions;
        private readonly IHttpService _httpService;
        private readonly ICacheService _cacheService;

        public OrderController(IOrderRepository orderRepository,
            IBankAccountRepository bankAccountRepository,
            IOptions<JPBankHubOption> jpBankHubOptions,
            IHttpService httpService,
            ICacheService cacheService)
        {
            _orderRepository = orderRepository;
            _bankAccountRepository = bankAccountRepository;
            _jpBankHubOptions = jpBankHubOptions;
            _httpService = httpService;
            _cacheService = cacheService;
        }

        #endregion Khởi tạo

        #region Hàm

        [HttpGet("~/order")]
        public async Task<IActionResult> FormCreateOrder()
        {
            var bankAccountSelectEles = await GetBankAccountSelectEleAsync();
            ViewData["BankAccountSelectEles"] = bankAccountSelectEles;
            return View("Create", new CreateOrderDto());
        }

        [HttpPost("~/order")]
        public async Task<IActionResult> CreateOrder(CreateOrderDto model)
        {
            var bankAccounts = await GetListBankAccountAsync();

            if (!ModelState.IsValid)
            {
                var bankAccountSelectEles = await GetBankAccountSelectEleAsync();
                ViewData["BankAccountSelectEles"] = bankAccountSelectEles;
                return View("Create", model);
            }

            var bankAccount = bankAccounts.FirstOrDefault(x => x.Id == model.BankAccountId);

            // tạo mới đơn hàng
            var order = new Order(GuidGenerator.Create())
            {
                OrderId = await GetNewOrderId(),
                OrderAmount = model.OrderAmount,
                Bank = new BankObject
                {
                    BankCode = bankAccount?.BankCode ?? string.Empty,
                    AccountNo = bankAccount?.AccountNo ?? string.Empty,
                },
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _orderRepository.InsertAsync(order, true);

            var accessToken = await GetJPToken();

            await JPBankHubCreateTransaction(accessToken, order);

            await _orderRepository.UpdateAsync(order, true);

            return RedirectToAction("OrderPaymentInfo", new { order.OrderId });
        }

        [HttpGet("~/order/{orderId}/payment")]
        public async Task<IActionResult> OrderPaymentInfo(int orderId)
        {
            var orderQueryable = await _orderRepository.GetQueryableAsync();
            orderQueryable = orderQueryable.Where(x => x.OrderId == orderId);
            var order = await AsyncExecuter.FirstOrDefaultAsync(orderQueryable);
            if (order == default)
            {
                return BadRequest();
            }

            ViewData["Order"] = order;
            return View("OrderPaymentInfo");
        }

        [HttpGet("~/orders")]
        public async Task<IActionResult> GetListAsync(int page = 1)
        {
            var orderQueryable = await _orderRepository.GetQueryableAsync();
            var totalCount = await AsyncExecuter.CountAsync(orderQueryable);

            var orders = new List<Order>();
            var limit = 10;

            if (totalCount > 0)
            {
                var start = (page - 1) * limit;

                orders = await AsyncExecuter.ToListAsync(orderQueryable.OrderByDescending(x => x.UpdatedDate).PageBy(start, limit));
            }
            var now = Clock.Now;
            var totalPages = (int)Math.Ceiling((decimal)totalCount / limit);
            ViewData["TotalPages"] = totalPages;
            ViewData["Page"] = page < 1 ? 1 : page;
            ViewData["Orders"] = orders;

            return View("List");
        }

        [HttpPost("~/order/jp/callback")]
        public async Task<dynamic> OrderJPCallback([FromBody] dynamic reqBody)
        {
            JpBankHubWebhookReqDto jpBankHubWebhookReqDto = JsonConvert.DeserializeObject<JpBankHubWebhookReqDto>(reqBody.ToString());

            Logger.LogDebug($"OrderController-OrderJPCallback-Req: {JsonConvert.SerializeObject(jpBankHubWebhookReqDto)}");

            var jpBankHubWebhookResDto = new JpBankHubWebhookResDto
            {
                Header = new JpBankHubWebhookHeaderResDto
                {
                    MsgId = jpBankHubWebhookReqDto.Header.MsgId,
                    ResponseMsgId = $"JP{DateTime.Now:yyyyMMddHHmmss}",
                    Signature = string.Empty
                },
                Data = new JpBankHubWebhookDataResDto
                {
                    Records = new List<JpBankHubWebhookDataRecordItemResDto>()
                }
            };

            // tạo chữ ký res
            var resSignData = $"{_jpBankHubOptions.Value.SecretKey}{jpBankHubWebhookResDto.Header.MsgId}{jpBankHubWebhookResDto.Header.ResponseMsgId}";
            jpBankHubWebhookResDto.Header.Signature = CreateMD5(resSignData);

            var signData = $"{_jpBankHubOptions.Value.SecretKey}{jpBankHubWebhookReqDto.Header.MsgId}";
            if (!CreateMD5(signData).Equals(jpBankHubWebhookReqDto.Header.Signature, StringComparison.OrdinalIgnoreCase))
            {
                jpBankHubWebhookResDto.Data.Records = jpBankHubWebhookReqDto.Data.Records.Select(x => new JpBankHubWebhookDataRecordItemResDto
                {
                    TransId = x.TransId,
                    Error = 3,
                    Message = "Sai chữ ký"
                }).ToList();

                Logger.LogDebug($"OrderController-OrderJPCallback-Res: {JsonConvert.SerializeObject(jpBankHubWebhookResDto)}");

                return jpBankHubWebhookResDto;
            }

            var orderQueryable = await _orderRepository.GetQueryableAsync();

            foreach (var trans in jpBankHubWebhookReqDto.Data.Records)
            {
                var validOrderId = int.TryParse(trans.OrderId, out var inputOrderId);
                if (!validOrderId)
                {
                    jpBankHubWebhookResDto.Data.Records.Add(new JpBankHubWebhookDataRecordItemResDto
                    {
                        TransId = trans.TransId,
                        Error = 1,
                        Message = "Thông tin đơn hàng không hợp lệ",
                    });

                    continue;
                }
                var orderItemQueryable = orderQueryable.Where(x => x.OrderId == inputOrderId)
                    .OrderByDescending(x => x.UpdatedDate);
                var order = await AsyncExecuter.FirstOrDefaultAsync(orderItemQueryable);
                if (order == default)
                {
                    jpBankHubWebhookResDto.Data.Records.Add(new JpBankHubWebhookDataRecordItemResDto
                    {
                        TransId = trans.TransId,
                        Error = 1,
                        Message = "Thông tin đơn hàng không hợp lệ",
                    });

                    continue;
                }

                if (order.OrderAmount > trans.Amount)
                {
                    jpBankHubWebhookResDto.Data.Records.Add(new JpBankHubWebhookDataRecordItemResDto
                    {
                        TransId = trans.TransId,
                        Error = 2,
                        Message = "Số tiền thanh toán nhỏ hơn số tiền thực đơn hàng",
                    });

                    continue;
                }

                if (order.OrderStatus != (int)OrderStatusType.PendingPayment && trans.OrderConfirmType == 1)
                {
                    jpBankHubWebhookResDto.Data.Records.Add(new JpBankHubWebhookDataRecordItemResDto
                    {
                        TransId = trans.TransId,
                        Error = 2,
                        Message = "Đơn hàng đã được xác nhận trước đó",
                    });

                    continue;
                }

                order.OrderStatus = (int)OrderStatusType.PaymentSuccess;
                order.UpdatedDate = DateTime.Now;

                jpBankHubWebhookResDto.Data.Records.Add(new JpBankHubWebhookDataRecordItemResDto
                {
                    TransId = trans.TransId,
                    Error = 0,
                    Message = "Xác nhận đơn hàng thành công",
                    OrderAmount = order.OrderAmount
                });

                await _orderRepository.UpdateAsync(order);
            }

            Logger.LogDebug($"OrderController-OrderJPCallback-Res: {JsonConvert.SerializeObject(jpBankHubWebhookResDto)}");

            return jpBankHubWebhookResDto;
        }

        #endregion Hàm

        #region Hàm private

        public static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return Convert.ToHexString(hashBytes); // .NET 5 +

                // Convert the byte array to hexadecimal string prior to .NET 5
                // StringBuilder sb = new System.Text.StringBuilder();
                // for (int i = 0; i < hashBytes.Length; i++)
                // {
                //     sb.Append(hashBytes[i].ToString("X2"));
                // }
                // return sb.ToString();
            }
        }

        private async Task JPBankHubCreateTransaction(string accessToken, Order order)
        {
            var jpBankHubCreateTransactionReqDto = new JPBankHubCreateTransactionReqDto
            {
                Org = new JPBankHubCreateTransactionOrgReqDto
                {
                    TaxNo = _jpBankHubOptions.Value.TaxNo,
                },
                Bank = new JPBankHubCreateTransactionBankReqDto
                {
                    BankCode = order.Bank.BankCode,
                    BankAccountNumber = order.Bank.AccountNo
                },
                Order = new JPBankHubCreateTransactionOrderReqDto
                {
                    OrderId = order.OrderId.ToString(),
                    Amount = order.OrderAmount
                }
            };

            var httpClientReqDto = new HttpClientReqDto
            {
                Url = $"{_jpBankHubOptions.Value.BusinessBaseUrl}{_jpBankHubOptions.Value.PathCreateTransaction}",
                Method = HttpMethod.Post,
                RequestBody = jpBankHubCreateTransactionReqDto,
                Headers = new Dictionary<string, string>
                {
                    { "Authorization", $"Bearer {accessToken}" }
                }
            };

            Logger.LogDebug($"OrderController-JPBankHubCreateTransaction-Req: {JsonConvert.SerializeObject(httpClientReqDto)}");

            var httpClientResDto = await _httpService.SendAsync(httpClientReqDto);

            Logger.LogDebug($"OrderController-JPBankHubCreateTransaction-Res: {JsonConvert.SerializeObject(httpClientResDto)}");

            if (httpClientResDto.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Khởi tạo giao dịch trên BankHub thất bại");
            }

            var jpBankHubCreateTransactionResDto = JsonConvert.DeserializeObject<JPBankHubCreateTransactionResDto>(httpClientResDto.ResponseBody);

            order.Bank.AccountName = jpBankHubCreateTransactionResDto.Bank.BankAccountName;
            order.Description = jpBankHubCreateTransactionResDto.Order.Description;
            order.VietQrUrl = jpBankHubCreateTransactionResDto.Order.VietQrUrl;
            order.UpdatedDate = DateTime.UtcNow;
        }

        private async Task<string> GetJPToken()
        {
            var accessToken = await GetJPTokenFromCache();
            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = await GetJPTokenFromApi();
                await _cacheService.SetAsync(GetCacheKeyJPToken(), accessToken, new Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(23)
                });
            }

            return accessToken;
        }

        private async Task<string> GetJPTokenFromApi()
        {
            var httpClientReqDto = new HttpClientReqDto
            {
                Url = $"{_jpBankHubOptions.Value.AuthBaseUrl}{_jpBankHubOptions.Value.PathGetToken}",
                Method = HttpMethod.Post,
                RequestBody = new Dictionary<string, string>
                {
                    { "client_id", _jpBankHubOptions.Value.ClientId },
                    { "client_secret", _jpBankHubOptions.Value.ClientSecret },
                    { "grant_type", _jpBankHubOptions.Value.GrantType }
                },
                ContentType = "application/x-www-form-urlencoded"
            };

            Logger.LogDebug($"OrderController-GetJPTokenFromApi-Req: {JsonConvert.SerializeObject(httpClientReqDto)}");

            var httpClientResDto = await _httpService.SendAsync(httpClientReqDto);

            Logger.LogDebug($"OrderController-GetJPTokenFromApi-Res: {JsonConvert.SerializeObject(httpClientResDto)}");

            if (httpClientResDto.StatusCode != HttpStatusCode.OK)
            {
                return string.Empty;
            }

            var jpBankHubTokenDto = Newtonsoft.Json.JsonConvert.DeserializeObject<JPBankHubTokenDto>(httpClientResDto.ResponseBody);
            return jpBankHubTokenDto.AccessToken;
        }

        private async Task<string> GetJPTokenFromCache()
        {
            var accessToken = await _cacheService.GetAsync(GetCacheKeyJPToken());
            return accessToken;
        }

        private string GetCacheKeyJPToken()
        {
            return "JPToken";
        }

        private async Task<List<BankAccount>> GetListBankAccountAsync()
        {
            var bankAccountQuerayble = await _bankAccountRepository.GetQueryableAsync();
            var bankAccounts = await AsyncExecuter.ToListAsync(bankAccountQuerayble);
            return bankAccounts;
        }

        private async Task<List<SelectListItem>> GetBankAccountSelectEleAsync()
        {
            var bankAccounts = await GetListBankAccountAsync();
            var selectItems = bankAccounts.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = $"{x.BankCode} - {x.AccountNo}"
            });
            return selectItems.ToList();
        }

        private async Task<int> GetNewOrderId()
        {
            var orderQueryable = await _orderRepository.GetQueryableAsync();
            var orderIdQueryable = orderQueryable
                .OrderByDescending(x => x.OrderId)
                .Select(x => x.OrderId);
            var latestOrderId = await AsyncExecuter.FirstOrDefaultAsync(orderIdQueryable);
            if (latestOrderId == default)
            {
                return 1;
            }
            return latestOrderId + 1;
        }

        #endregion Hàm private
    }
}