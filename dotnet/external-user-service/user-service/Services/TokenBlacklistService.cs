using Microsoft.Extensions.Caching.Memory;
using AspNetJwtAuth.Services.Interfaces;

namespace AspNetJwtAuth.Services
{
    public class TokenBlacklistService : ITokenBlacklistService
    {
        private readonly IMemoryCache _cache;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenBlacklistService> _logger;
        private const string BlacklistCachePrefix = "blacklist_";
        private readonly TimeSpan _cacheExpiry;

        public TokenBlacklistService(
            IMemoryCache cache,
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<TokenBlacklistService> logger)
        {
            _cache = cache;
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            
            var cacheMinutes = configuration.GetValue<int>("TokenBlacklist:CacheTtlMinutes", 10);
            _cacheExpiry = TimeSpan.FromMinutes(cacheMinutes);
            
            // Configure HTTP client
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<bool> IsTokenBlacklistedAsync(string jti)
        {
            if (string.IsNullOrEmpty(jti))
            {
                _logger.LogWarning("Empty JTI provided for blacklist check");
                return false;
            }

            var cacheKey = $"{BlacklistCachePrefix}{jti}";
            
            // Check local cache first
            if (_cache.TryGetValue(cacheKey, out bool cachedResult))
            {
                _logger.LogDebug("Cache hit for token blacklist check: {Jti} -> {IsBlacklisted}", jti, cachedResult);
                return cachedResult;
            }

            // Check with external service
            bool isBlacklisted = await CheckBlacklistWithExternalServiceAsync(jti);
            
            // Cache the result
            _cache.Set(cacheKey, isBlacklisted, _cacheExpiry);
            
            _logger.LogDebug("Token blacklist check: {Jti} -> {IsBlacklisted} (cached for {CacheMinutes} minutes)", 
                jti, isBlacklisted, _cacheExpiry.TotalMinutes);
            
            return isBlacklisted;
        }

        private async Task<bool> CheckBlacklistWithExternalServiceAsync(string jti)
        {
            // For single-service deployment, skip external calls to avoid circular dependency
            // In production, this would call an external service or shared blacklist store
            _logger.LogDebug("Skipping external blacklist check for single-service deployment: {Jti}", jti);
            await Task.CompletedTask; // Keep async signature for future use
            return false; // Token is not blacklisted if not in local cache
        }

        public async Task BlacklistTokenAsync(string jti, TimeSpan expiry)
        {
            if (string.IsNullOrEmpty(jti))
            {
                _logger.LogWarning("Empty JTI provided for blacklisting");
                return;
            }

            try
            {
                var cacheKey = $"{BlacklistCachePrefix}{jti}";
                
                // Cache locally
                _cache.Set(cacheKey, true, expiry);
                _logger.LogInformation("Token {Jti} blacklisted locally for {ExpiryMinutes} minutes", 
                    jti, expiry.TotalMinutes);

                // Optionally notify external service
                await NotifyExternalServiceAsync(jti, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to blacklist token {Jti}", jti);
                throw;
            }
        }

        private async Task NotifyExternalServiceAsync(string jti, TimeSpan expiry)
        {
            // For single-service deployment, skip external notifications
            // In production, this would notify external services or shared stores
            _logger.LogDebug("Skipping external blacklist notification for single-service deployment: {Jti}", jti);
            await Task.CompletedTask; // Keep async signature for future use
        }

        public async Task<int> GetBlacklistedTokenCountAsync()
        {
            // This is a simplified implementation for demo purposes
            // In a real application, you might want to track this more comprehensively
            await Task.CompletedTask; // Placeholder for async consistency
            
            // For in-memory cache, we can't easily count entries without reflection
            // This would typically be implemented with a distributed cache like Redis
            _logger.LogDebug("Blacklisted token count requested (not implemented for MemoryCache)");
            return 0;
        }
    }
}
