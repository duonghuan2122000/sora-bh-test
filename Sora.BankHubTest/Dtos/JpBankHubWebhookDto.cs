using Newtonsoft.Json;

namespace Sora.BankHubTest.Dtos
{
    public partial class JpBankHubWebhookReqDto
    {
        [JsonProperty("header")]
        public JpBankHubWebhookHeaderReqDto Header { get; set; }

        [JsonProperty("data")]
        public JpBankHubWebhookDataReqDto Data { get; set; }
    }

    public partial class JpBankHubWebhookDataReqDto
    {
        [JsonProperty("records")]
        public JpBankHubWebhookDataRecordItemReqDto[] Records { get; set; }
    }

    public partial class JpBankHubWebhookDataRecordItemReqDto
    {
        [JsonProperty("orderConfirmType")]
        public int OrderConfirmType { get; set; }

        [JsonProperty("orderId")]
        public string OrderId { get; set; }

        [JsonProperty("amount")]
        public long Amount { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("transId")]
        public string TransId { get; set; }

        [JsonProperty("bankTransId")]
        public string BankTransId { get; set; }
    }

    public partial class JpBankHubWebhookHeaderReqDto
    {
        [JsonProperty("msgId")]
        public string MsgId { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }
    }

    public partial class JpBankHubWebhookResDto
    {
        [JsonProperty("header")]
        public JpBankHubWebhookHeaderResDto Header { get; set; }

        [JsonProperty("data")]
        public JpBankHubWebhookDataResDto Data { get; set; }
    }

    public partial class JpBankHubWebhookDataResDto
    {
        [JsonProperty("records")]
        public List<JpBankHubWebhookDataRecordItemResDto> Records { get; set; } = new List<JpBankHubWebhookDataRecordItemResDto>();
    }

    public partial class JpBankHubWebhookDataRecordItemResDto
    {
        [JsonProperty("transId")]
        public string TransId { get; set; }

        [JsonProperty("error")]
        public long Error { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("orderAmount")]
        public long? OrderAmount { get; set; }
    }

    public partial class JpBankHubWebhookHeaderResDto
    {
        [JsonProperty("msgId")]
        public string MsgId { get; set; }

        [JsonProperty("responseMsgId")]
        public string ResponseMsgId { get; set; }

        [JsonProperty("signature")]
        public string Signature { get; set; }
    }
}
