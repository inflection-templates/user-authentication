using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text.Json;
using AspNetJwtAuth.Models;
using AspNetJwtAuth.Services.Interfaces;

namespace AspNetJwtAuth.Services
{
    public class JwksService : IJwksService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwksService> _logger;
        private const string CacheKey = "jwks_keys";
        private readonly TimeSpan _cacheExpiry;

        public JwksService(
            HttpClient httpClient,
            IMemoryCache cache,
            IConfiguration configuration,
            ILogger<JwksService> logger)
        {
            _httpClient = httpClient;
            _cache = cache;
            _configuration = configuration;
            _logger = logger;
            
            var cacheMinutes = configuration.GetValue<int>("Jwt:KeysCacheTtlMinutes", 60);
            _cacheExpiry = TimeSpan.FromMinutes(cacheMinutes);
            
            // Configure HTTP client
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "AspNetJwtAuth/1.0");
        }

        public async Task<IEnumerable<SecurityKey>> GetSigningKeysAsync(string? kid = null)
        {
            try
            {
                var keys = await GetCachedKeysAsync();
                
                if (!string.IsNullOrEmpty(kid))
                {
                    var filteredKeys = keys.Where(k => k.KeyId == kid).ToList();
                    _logger.LogDebug("Filtered {FilteredCount} keys for kid: {Kid} from {TotalCount} total keys", 
                        filteredKeys.Count, kid, keys.Count);
                    return filteredKeys;
                }
                
                _logger.LogDebug("Returning {KeyCount} signing keys", keys.Count);
                return keys;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get signing keys for kid: {Kid}", kid);
                return new List<SecurityKey>();
            }
        }

        private async Task<List<SecurityKey>> GetCachedKeysAsync()
        {
            if (_cache.TryGetValue(CacheKey, out List<SecurityKey>? cachedKeys) && cachedKeys != null)
            {
                _logger.LogDebug("Retrieved {KeyCount} keys from cache", cachedKeys.Count);
                return cachedKeys;
            }

            _logger.LogInformation("Cache miss, fetching keys from JWKS endpoint");
            var keys = await FetchKeysFromJwksEndpointAsync();
            
            if (keys.Any())
            {
                _cache.Set(CacheKey, keys, _cacheExpiry);
                _logger.LogInformation("Cached {KeyCount} keys for {ExpiryMinutes} minutes", 
                    keys.Count, _cacheExpiry.TotalMinutes);
            }
            
            return keys;
        }

        private async Task<List<SecurityKey>> FetchKeysFromJwksEndpointAsync()
        {
            try
            {
                var jwksUrl = _configuration["Jwt:JwksUrl"];
                if (string.IsNullOrEmpty(jwksUrl))
                {
                    _logger.LogError("JWKS URL not configured");
                    return new List<SecurityKey>();
                }

                _logger.LogDebug("Fetching JWKS from: {JwksUrl}", jwksUrl);
                
                var response = await _httpClient.GetStringAsync(jwksUrl);
                var jwksResponse = JsonSerializer.Deserialize<JwksResponse>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (jwksResponse?.Keys == null || !jwksResponse.Keys.Any())
                {
                    _logger.LogWarning("No keys found in JWKS response");
                    return new List<SecurityKey>();
                }

                var securityKeys = new List<SecurityKey>();

                foreach (var key in jwksResponse.Keys)
                {
                    try
                    {
                        if (key.Kty?.ToUpperInvariant() == "RSA")
                        {
                            var rsaKey = CreateRsaSecurityKey(key);
                            if (rsaKey != null)
                            {
                                securityKeys.Add(rsaKey);
                                _logger.LogDebug("Added RSA key with kid: {Kid}", key.Kid);
                            }
                        }
                        else if (key.Kty?.ToUpperInvariant() == "EC")
                        {
                            _logger.LogDebug("EC keys not currently supported, skipping kid: {Kid}", key.Kid);
                        }
                        else
                        {
                            _logger.LogDebug("Unsupported key type: {Kty} for kid: {Kid}", key.Kty, key.Kid);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to process key with kid: {Kid}", key.Kid);
                    }
                }

                _logger.LogInformation("Successfully fetched {KeyCount} keys from JWKS endpoint", securityKeys.Count);
                return securityKeys;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error while fetching JWKS keys");
                return new List<SecurityKey>();
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, "Timeout while fetching JWKS keys");
                return new List<SecurityKey>();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to parse JWKS response");
                return new List<SecurityKey>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching JWKS keys");
                return new List<SecurityKey>();
            }
        }

        private RsaSecurityKey? CreateRsaSecurityKey(AspNetJwtAuth.Models.JsonWebKey key)
        {
            try
            {
                if (string.IsNullOrEmpty(key.N) || string.IsNullOrEmpty(key.E))
                {
                    _logger.LogWarning("RSA key missing required parameters (n or e) for kid: {Kid}", key.Kid);
                    return null;
                }

                var rsa = RSA.Create();
                rsa.ImportParameters(new RSAParameters
                {
                    Modulus = Base64UrlEncoder.DecodeBytes(key.N),
                    Exponent = Base64UrlEncoder.DecodeBytes(key.E)
                });

                return new RsaSecurityKey(rsa)
                {
                    KeyId = key.Kid
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create RSA security key for kid: {Kid}", key.Kid);
                return null;
            }
        }

        public async Task RefreshKeysAsync()
        {
            try
            {
                _logger.LogInformation("Manually refreshing JWKS keys");
                _cache.Remove(CacheKey);
                await GetCachedKeysAsync();
                _logger.LogInformation("JWKS keys refresh completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh JWKS keys");
                throw;
            }
        }

        public async Task<int> GetCachedKeyCountAsync()
        {
            try
            {
                var keys = await GetCachedKeysAsync();
                return keys.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get cached key count");
                return 0;
            }
        }
    }
}
