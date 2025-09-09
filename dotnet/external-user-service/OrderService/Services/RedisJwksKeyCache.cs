using StackExchange.Redis;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text.Json;

namespace OrderService.Services
{
    public class RedisJwksKeyCache : IJwksKeyCache
    {
        private readonly IDatabase _db;
        private const string Prefix = "jwks:kid:";

        public RedisJwksKeyCache(IConnectionMultiplexer mux)
        {
            _db = mux.GetDatabase();
        }

        public async Task<CachedSecurityKey?> TryGetAsync(string kid)
        {
            var json = await _db.StringGetAsync(Prefix + kid);
            if (json.IsNullOrEmpty) return null;

            var jwk = JsonSerializer.Deserialize<JwksKey>(json!);
            if (jwk == null) return null;

            var rsa = RSA.Create();
            rsa.ImportParameters(new RSAParameters
            {
                Modulus = Base64UrlEncoder.DecodeBytes(jwk.N),
                Exponent = Base64UrlEncoder.DecodeBytes(jwk.E)
            });
            var key = new RsaSecurityKey(rsa) { KeyId = jwk.Kid };
            return new CachedSecurityKey(key, DateTime.UtcNow);
        }

        public async Task SetAsync(string kid, JwksKey jwk, TimeSpan ttl)
        {
            var json = JsonSerializer.Serialize(jwk);
            await _db.StringSetAsync(Prefix + kid, json, ttl);
        }

        public Task RemoveAsync(string kid)
        {
            return _db.KeyDeleteAsync(Prefix + kid);
        }
    }
}


