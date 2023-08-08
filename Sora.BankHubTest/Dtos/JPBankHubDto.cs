using Newtonsoft.Json;
using Sora.BankHubTest.Entities;

namespace Sora.BankHubTest.Dtos
{
    public class JPBankHubTokenDto
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; } = 86400;
    }

    public class JPBankHubCreateTransactionReqDto
    {
        [JsonProperty("org")]
        public JPBankHubCreateTransactionOrgReqDto Org { get; set; }

        [JsonProperty("bank")]
        public JPBankHubCreateTransactionBankReqDto Bank { get; set; }

        [JsonProperty("order")]
        public JPBankHubCreateTransactionOrderReqDto Order { get; set; }
    }

    public partial class JPBankHubCreateTransactionBankReqDto
    {
        [JsonProperty("bankCode")]
        public string BankCode { get; set; }

        [JsonProperty("bankAccountNumber")]
        public string BankAccountNumber { get; set; }
    }

    public partial class JPBankHubCreateTransactionOrderReqDto
    {
        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("amount")]
        public long Amount { get; set; }
    }

    public partial class JPBankHubCreateTransactionOrgReqDto
    {
        [JsonProperty("taxNo")]
        public string TaxNo { get; set; }
    }

    public class JPBankHubCreateTransactionResDto
    {
        [JsonProperty("bank")]
        public JPBankHubCreateTransactionBankResDto Bank { get; set; }

        [JsonProperty("order")]
        public JPBankHubCreateTransactionOrderResDto Order { get; set; }
    }

    public partial class JPBankHubCreateTransactionBankResDto
    {
        [JsonProperty("bankCode")]
        public string BankCode { get; set; }

        [JsonProperty("bankName")]
        public string BankName { get; set; }

        [JsonProperty("bankAccountNumber")]
        public string BankAccountNumber { get; set; }

        [JsonProperty("bankAccountName")]
        public string BankAccountName { get; set; }

        [JsonProperty("branchName")]
        public string BranchName { get; set; }
    }

    public partial class JPBankHubCreateTransactionOrderResDto
    {
        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("amount")]
        public long Amount { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("vietQrUrl")]
        public string VietQrUrl { get; set; }
    }
}