using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text.Json;
using System.Collections.Concurrent;

namespace OrderService.Services
{
    public interface IJwtAuthenticationService
    {
        Task<List<SecurityKey>> GetSigningKeysAsync(string kid);
        Task RefreshKeysAsync();
    }

    public class JwtAuthenticationService : IJwtAuthenticationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<JwtAuthenticationService> _logger;
        private readonly string _jwksUrl;
        private readonly IJwksKeyCache _jwksCache;
        private readonly SemaphoreSlim _refreshSemaphore;
        private DateTime _lastRefresh = DateTime.MinValue;
        private readonly TimeSpan _refreshInterval = TimeSpan.FromMinutes(5);

        public JwtAuthenticationService(HttpClient httpClient, ILogger<JwtAuthenticationService> logger, IConfiguration configuration, IJwksKeyCache jwksCache)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jwksUrl = configuration["Jwt:JwksUrl"] ?? throw new ArgumentException("JwksUrl not configured");
            _jwksCache = jwksCache;
            _refreshSemaphore = new SemaphoreSlim(1, 1);
        }

        public async Task<List<SecurityKey>> GetSigningKeysAsync(string kid)
        {
            // Check cache first
            var cachedKey = await _jwksCache.TryGetAsync(kid);
            if (cachedKey != null && !cachedKey.IsExpired)
            {
                _logger.LogDebug("Using cached key for kid: {Kid}", kid);
                return new List<SecurityKey> { cachedKey.SecurityKey };
            }

            // Check if we need to refresh keys
            if (ShouldRefreshKeys())
            {
                await RefreshKeysAsync();
            }

            // Try cache again after refresh
            cachedKey = await _jwksCache.TryGetAsync(kid);
            if (cachedKey != null && !cachedKey.IsExpired)
            {
                _logger.LogDebug("Using refreshed cached key for kid: {Kid}", kid);
                return new List<SecurityKey> { cachedKey.SecurityKey };
            }

            _logger.LogWarning("No valid key found for kid: {Kid}", kid);
            return new List<SecurityKey>();
        }

        public async Task RefreshKeysAsync()
        {
            await _refreshSemaphore.WaitAsync();
            try
            {
                _logger.LogInformation("Refreshing JWKS keys from: {JwksUrl}", _jwksUrl);

                var response = await _httpClient.GetStringAsync(_jwksUrl);
                var jwks = JsonSerializer.Deserialize<JwksResponse>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (jwks?.Keys != null)
                {
                    _logger.LogInformation("Received {KeyCount} keys from JWKS", jwks.Keys.Count);

                    // Add or update keys in distributed cache
                    foreach (var jwksKey in jwks.Keys)
                    {
                        await _jwksCache.SetAsync(jwksKey.Kid, jwksKey, TimeSpan.FromMinutes(10));
                        _logger.LogDebug("Cached key with kid: {Kid}", jwksKey.Kid);
                    }

                    _lastRefresh = DateTime.UtcNow;
                    _logger.LogInformation("Successfully refreshed and cached keys");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing JWKS keys");
            }
            finally
            {
                _refreshSemaphore.Release();
            }
        }

        private bool ShouldRefreshKeys()
        {
            return DateTime.UtcNow - _lastRefresh > _refreshInterval;
        }

        internal static SecurityKey CreateSecurityKeyStatic(JwksKey jwksKey)
        {
            var rsa = RSA.Create();
            rsa.ImportParameters(new RSAParameters
            {
                Modulus = Base64UrlEncoder.DecodeBytes(jwksKey.N),
                Exponent = Base64UrlEncoder.DecodeBytes(jwksKey.E)
            });
            return new RsaSecurityKey(rsa) { KeyId = jwksKey.Kid };
        }

        private SecurityKey CreateSecurityKey(JwksKey jwksKey) => CreateSecurityKeyStatic(jwksKey);
    }

    public class CachedSecurityKey
    {
        public SecurityKey SecurityKey { get; }
        public DateTime CachedAt { get; }
        public DateTime ExpiresAt { get; }

        public bool IsExpired => DateTime.UtcNow > ExpiresAt;

        public CachedSecurityKey(SecurityKey securityKey, DateTime cachedAt)
        {
            SecurityKey = securityKey;
            CachedAt = cachedAt;
            ExpiresAt = cachedAt.AddMinutes(10); // Keys expire after 10 minutes
        }
    }

    public class JwksResponse
    {
        public List<JwksKey> Keys { get; set; } = new();
    }

    public class JwksKey
    {
        public string Kty { get; set; } = string.Empty;
        public string Use { get; set; } = string.Empty;
        public string Kid { get; set; } = string.Empty;
        public string N { get; set; } = string.Empty;
        public string E { get; set; } = string.Empty;
    }
}
