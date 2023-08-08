using Microsoft.Extensions.Caching.Distributed;

namespace Sora.BankHubTest.Caches
{
    public interface ICacheService
    {
        Task<string> GetAsync(string key);

        Task SetAsync(string key, string value, DistributedCacheEntryOptions options = default);

        Task RemoveAsync(string key);
    }
}