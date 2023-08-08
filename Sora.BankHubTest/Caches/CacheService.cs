using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using Volo.Abp.DependencyInjection;

namespace Sora.BankHubTest.Caches
{
    public class CacheService : ICacheService, ISingletonDependency
    {
        private readonly IDistributedCache _cache;

        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<string> GetAsync(string key)
        {
            try
            {
                var value = await _cache.GetStringAsync(key);
                return value;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "CacheService-GetAsync-Exception");
            }

            return null;
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "CacheService-RemoveAsync-Exception");
            }
        }

        public async Task SetAsync(string key, string value, DistributedCacheEntryOptions options = default)
        {
            try
            {
                await _cache.SetStringAsync(key, value, options);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "CacheService-SetAsync-Exception");
            }
        }
    }
}