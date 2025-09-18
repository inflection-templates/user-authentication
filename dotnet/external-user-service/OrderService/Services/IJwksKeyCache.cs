using Microsoft.IdentityModel.Tokens;

namespace OrderService.Services
{
    public interface IJwksKeyCache
    {
        Task<CachedSecurityKey?> TryGetAsync(string kid);
        Task SetAsync(string kid, JwksKey jwk, TimeSpan ttl);
        Task RemoveAsync(string kid);
    }
}


