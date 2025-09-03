using System.Text.Json;
using shala.api.common;
using shala.api.database.interfaces;
using shala.api.domain.types;
using shala.api.modules.cache;
using shala.api.startup;

namespace shala.api.services;

public class ApiKeyService : BaseService<ApiKeyService>, IApiKeyService
{

    #region Constructor

    private readonly IApiKeyRepository _apiKeyRepository;
    private readonly IClientAppRepository _clientAppRepository;

    public ApiKeyService(
        IConfiguration configuration,
        IApiKeyRepository apiKeyRepository,
        IClientAppRepository clientAppRepository,
        ILogger<ApiKeyService> logger,
        ICacheService cacheService)
        : base(configuration, logger, cacheService)
    {
        _apiKeyRepository = apiKeyRepository;
        _clientAppRepository = clientAppRepository;
    }

    public async Task<ApiKey?> CreateAsync(ApiKeyCreateModel model)
    {
        return await TraceAsync("CreateAsync", async () =>
        {
            var apiKey = await _apiKeyRepository.CreateAsync(model);
            if (!CacheEnabled)
            {
                return apiKey;
            }
            if (apiKey != null)
            {
                Scheduler.FireAndForget(() => InvalidateSearchCache());
            }
            return apiKey;
        });
    }

    public async Task<ApiKey?> GetByIdAsync(Guid id)
    {
        return await TraceAsync("GetByIdAsync", async () =>
        {
            if (!CacheEnabled)
            {
                return await _apiKeyRepository.GetByIdAsync(id);
            }
            var cacheKey = $"ApiKeyService->GetByIdAsync->id:{id}";
            var cached = await _cacheService.GetAsync<ApiKey>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Retrieved api key from cache");
                return cached;
            }
            var apiKey = await _apiKeyRepository.GetByIdAsync(id);
            await _cacheService.SetAsync(cacheKey, apiKey);
            return apiKey;
        });
    }

    public async Task<List<ApiKey>> GetByClientAppIdAsync(Guid clientAppId)
    {
        return await TraceAsync("GetByClientAppIdAsync", async () =>
        {
            if (!CacheEnabled)
            {
                return await _apiKeyRepository.GetByClientAppIdAsync(clientAppId);
            }
            var cacheKey = $"ApiKeyService->GetByClientAppIdAsync->clientAppId:{clientAppId}";
            var cached = await _cacheService.GetAsync<List<ApiKey>>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Retrieved api key from cache");
                return cached;
            }
            var apiKeys = await _apiKeyRepository.GetByClientAppIdAsync(clientAppId);
            await _cacheService.SetAsync(cacheKey, apiKeys);
            return apiKeys;
        });
    }

    public async Task<ApiKey?> GetByKeyAsync(string key)
    {
        return await TraceAsync("GetByKeyAsync", async () =>
        {
            if (!CacheEnabled)
            {
                return await _apiKeyRepository.GetByKeyAsync(key);
            }
            var cacheKey = $"ApiKeyService->GetByKeyAsync->key:{key}";
            var cached = await _cacheService.GetAsync<ApiKey>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Retrieved api key from cache");
                return cached;
            }
            var apiKey = await _apiKeyRepository.GetByKeyAsync(key);
            await _cacheService.SetAsync(cacheKey, apiKey);
            return apiKey;
        });
    }

    public async Task<ClientApp?> ValidateAsync(string key, string secret)
    {
        return await TraceAsync("ValidateAsync", async () =>
        {
            var apiKey = await _apiKeyRepository.GetByKeyAsync(key);
            if (apiKey == null)
            {
                throw new InvalidApiKeyException();
            }
            if (apiKey.ValidTill.HasValue && apiKey.ValidTill.Value < DateTime.UtcNow)
            {
                throw new ExpiredApiKeyException();
            }
            var secretHash = await _apiKeyRepository.GetSecretHashAsync(key);
            if (secretHash == null)
            {
                throw new InvalidApiKeyException();
            }
            var isValid = BCrypt.Net.BCrypt.Verify(secret, secretHash);
            if (!isValid)
            {
                return null;
            }
            return await _clientAppRepository.GetByIdAsync(apiKey.ClientAppId);
        });
    }

    public async Task<ClientApp?> GetClientAppByKeyAsync(string key)
    {
        return await TraceAsync("GetClientAppByKeyAsync", async () =>
        {
            if (CacheEnabled)
            {
                var cacheKey = $"ApiKeyService->GetClientAppByKeyAsync->key:{key}";
                var cached = await _cacheService.GetAsync<ClientApp>(cacheKey);
                if (cached != null)
                {
                    _logger.LogInformation("Retrieved client app from cache");
                    return cached;
                }
                var apiKey = await _apiKeyRepository.GetByKeyAsync(key);
                if (apiKey == null)
                {
                    return null;
                }
                var clientApp = await _clientAppRepository.GetByIdAsync(apiKey.ClientAppId);
                await _cacheService.SetAsync(cacheKey, clientApp);
                return clientApp;
            }
            else
            {
                var apiKey = await _apiKeyRepository.GetByKeyAsync(key);
                if (apiKey == null)
                {
                    return null;
                }
                return await _clientAppRepository.GetByIdAsync(apiKey.ClientAppId);
            }
        });
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await TraceAsync("DeleteAsync", async () =>
        {
            var apiKey = await _apiKeyRepository.GetByIdAsync(id);
            if (apiKey == null)
            {
                return false;
            }
            var deleted = await _apiKeyRepository.DeleteAsync(id);
            if (CacheEnabled && deleted)
            {
                Scheduler.FireAndForget(() => InvalidateApiKeyCache(apiKey));
            }
            return deleted;
        });
    }

    public async Task<bool> DeleteByClientAppIdAsync(Guid clientAppId)
    {
        return await TraceAsync("DeleteByClientAppIdAsync", async () =>
        {
            var apiKeys = await _apiKeyRepository.GetByClientAppIdAsync(clientAppId);
            if (apiKeys == null)
            {
                return false;
            }
            var deleted = await _apiKeyRepository.DeleteByClientAppIdAsync(clientAppId);
            if (CacheEnabled && deleted)
            {
                for (var i = 0; i < apiKeys.Count; i++)
                {
                    Scheduler.FireAndForget(() => InvalidateApiKeyCache(apiKeys[i]));
                }
            }
            return deleted;
        });
    }

    public async Task<SearchResults<ApiKey>> SearchAsync(ApiKeySearchFilters filters)
    {
        return await TraceAsync("SearchAsync", async () =>
        {
            if (!CacheEnabled)
            {
                return await _apiKeyRepository.SearchAsync(filters);
            }
            var cacheKey = $"apiKeysService->SearchAsync->{JsonSerializer.Serialize<ApiKeySearchFilters>(filters)}";
            var cachedResults = await _cacheService.GetAsync<SearchResults<ApiKey>>(cacheKey);
            if (cachedResults != null)
            {
                _logger.LogInformation("Retrieved apiKeys search results from cache");
                return cachedResults;
            }
            var results = await _apiKeyRepository.SearchAsync(filters);
            await _cacheService.SetAsync(cacheKey, results);
            return results;
        });
    }

    public void InvalidateApiKeyCache(ApiKey? apiKey)
    {
        if (!CacheEnabled || apiKey == null)
        {
            return;
        }

        _logger.LogInformation("Invalidating all api key cache entries");
        _cacheService.FindAndClear<ApiKey>("ApiKeyService->SearchAsync");

        var cacheKeys = new List<string>() {
                $"ApiKeyService->GetByIdAsync->id:{apiKey.Id}",
                $"ApiKeyService->GetByKeyAsync->key:{apiKey.Key}",
                $"ApiKeyService->GetClientAppByKeyAsync->key:{apiKey.Key}",
                $"ApiKeyService->GetByClientAppIdAsync->clientAppId:{apiKey.ClientAppId}",
            };
        _cacheService.Invalidate(cacheKeys);
    }

    public void InvalidateSearchCache()
    {
        if (!CacheEnabled)
        {
            return;
        }
        _logger.LogInformation("Invalidating all Api search cache entries");
        _cacheService.FindAndClear<ApiKey>("ApiKeyService->SearchAsync");
    }

    #endregion

}
