using Identity.Application.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace Identity.Infrastructure.Services
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly IDistributedCache _cache;
        public TokenBlacklistService(IDistributedCache cache)
        {
            _cache = cache;
        }
        public async Task BlacklistTokenAsync(string token, TimeSpan expiry)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry
            };

            await _cache.SetStringAsync($"blacklist:{token}", "true", options);
        }

        public async Task<bool> IsBlacklistedAsync(string token)
        {
            var value = await _cache.GetStringAsync($"blacklist:{token}");
            return value != null;
        }
    }
}
