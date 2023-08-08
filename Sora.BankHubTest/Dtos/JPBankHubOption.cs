namespace Sora.BankHubTest.Dtos
{
    public class JPBankHubOption
    {
        public const string Key = "JPBankHub";

        public string AuthBaseUrl { get; set; }

        public string BusinessBaseUrl { get; set; }

        public string PathGetToken { get; set; }

        public string PathCreateTransaction { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string GrantType { get; set; }

        public string TaxNo { get; set; }

        public string SecretKey { get; set; }
    }
}