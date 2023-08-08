using Sora.BankHubTest.Https.Dtos;

namespace Sora.BankHubTest.Https
{
    public interface IHttpService
    {
        Task<HttpClientResDto> SendAsync(HttpClientReqDto httpClientReqDto);
    }
}