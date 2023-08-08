using System.Net;

namespace Sora.BankHubTest.Https.Dtos
{
    public class HttpClientReqDto
    {
        public string Url { get; set; }

        public HttpMethod Method { get; set; }

        public object RequestBody { get; set; }

        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        public int Timeout { get; set; } = 30;

        public string ContentType { get; set; } = "application/json";
    }

    public class HttpClientResDto
    {
        public HttpStatusCode StatusCode { get; set; }

        public string ResponseBody { get; set; }

        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
    }
}