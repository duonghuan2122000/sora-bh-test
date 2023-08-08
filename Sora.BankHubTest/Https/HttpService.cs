using Newtonsoft.Json;
using Serilog;
using Sora.BankHubTest.Https.Dtos;
using System.Text;

namespace Sora.BankHubTest.Https
{
    public class HttpService : IHttpService
    {
        #region Khởi tạo

        private readonly HttpClient _httpClient;

        public HttpService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        #endregion Khởi tạo

        #region Hàm

        public async Task<HttpClientResDto> SendAsync(HttpClientReqDto httpClientReqDto)
        {
            var httpClientResDto = new HttpClientResDto();

            try
            {
                using var httpRequestMessage = new HttpRequestMessage
                {
                    Method = httpClientReqDto.Method
                };

                string url = httpClientReqDto.Url;

                if (httpClientReqDto.Method == HttpMethod.Get)
                {
                    if (httpClientReqDto.RequestBody is Dictionary<string, string>)
                    {
                        url += "?";
                        foreach (var pairQuery in (Dictionary<string, string>)httpClientReqDto.RequestBody)
                        {
                            url += $"{pairQuery.Key}={pairQuery.Value}";
                        }
                    }
                }
                else
                {
                    switch (httpClientReqDto.ContentType)
                    {
                        case "application/x-www-form-urlencoded":
                            var reqBody = (Dictionary<string, string>)httpClientReqDto.RequestBody;
                            var parameters = new List<KeyValuePair<string, string>>();
                            foreach (var pair in reqBody)
                            {
                                parameters.Add(pair);
                            }
                            httpRequestMessage.Content = new FormUrlEncodedContent(parameters);
                            break;

                        default:
                            if (httpClientReqDto.RequestBody is string)
                            {
                                httpRequestMessage.Content = new StringContent(httpClientReqDto.RequestBody as string, Encoding.UTF8, httpClientReqDto.ContentType);
                            }
                            else
                            {
                                httpRequestMessage.Content = new StringContent(JsonConvert.SerializeObject(httpClientReqDto.RequestBody), Encoding.UTF8, httpClientReqDto.ContentType);
                            }
                            break;
                    }
                }

                if (httpClientReqDto.Headers != null && httpClientReqDto.Headers.Count > 0)
                {
                    foreach (var kvp in httpClientReqDto.Headers)
                    {
                        httpRequestMessage.Headers.Add(kvp.Key, kvp.Value);
                    }
                }
                httpRequestMessage.RequestUri = new Uri(url);
                using var response = await _httpClient.SendAsync(httpRequestMessage);

                httpClientResDto.StatusCode = response.StatusCode;
                httpClientResDto.ResponseBody = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "HttpService-SendAsync-Exception");
            }

            return httpClientResDto;
        }

        #endregion Hàm
    }
}