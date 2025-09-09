using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;

namespace OrderService.Services
{
    public class InMemoryJwksKeyCache : IJwksKeyCache
    {
        private class CacheEntry
        {
            public JwksKey Jwk { get; set; } = new JwksKey();
            public DateTime ExpiresAt { get; set; }
        }

        private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

        public Task<CachedSecurityKey?> TryGetAsync(string kid)
        {
            if (_cache.TryGetValue(kid, out var entry))
            {
                if (DateTime.UtcNow <= entry.ExpiresAt)
                {
                    var rsaKey = JwtAuthenticationService.CreateSecurityKeyStatic(entry.Jwk);
                    return Task.FromResult<CachedSecurityKey?>(new CachedSecurityKey(rsaKey, DateTime.UtcNow));
                }
                _cache.TryRemove(kid, out _);
            }
            return Task.FromResult<CachedSecurityKey?>(null);
        }

        public Task SetAsync(string kid, JwksKey jwk, TimeSpan ttl)
        {
            _cache[kid] = new CacheEntry
            {
                Jwk = jwk,
                ExpiresAt = DateTime.UtcNow.Add(ttl)
            };
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string kid)
        {
            _cache.TryRemove(kid, out _);
            return Task.CompletedTask;
        }
    }
}


