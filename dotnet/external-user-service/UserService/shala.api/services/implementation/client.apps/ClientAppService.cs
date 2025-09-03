using System.Text.Json;
using shala.api.database.interfaces;
using shala.api.domain.types;
using shala.api.modules.cache;
using shala.api.startup;

namespace shala.api.services;

public class ClientAppService : BaseService<ClientAppService>, IClientAppService
{

    #region Constructor

    private readonly IClientAppRepository _clientAppRepository;

    public ClientAppService(
          IConfiguration configuration,
          IClientAppRepository clientAppRepository,
          ILogger<ClientAppService> logger,
          ICacheService cacheService)
          : base(configuration, logger, cacheService)
      {
          _clientAppRepository = clientAppRepository;
      }

    #endregion

    public async Task<ClientApp?> GetByIdAsync(Guid id)
    {
      return await TraceAsync("GetByIdAsync", async () =>
          {
              if (!CacheEnabled)
              {
                  return await _clientAppRepository.GetByIdAsync(id);
              }
              var cacheKey = $"ClientAppService->GetByIdAsync->id:{id}";
              var cachedUser = await _cacheService.GetAsync<ClientApp>(cacheKey);
              if (cachedUser != null)
              {
                  _logger.LogInformation("Retrieved user from cache");
                  return cachedUser;
              }
              var user = await _clientAppRepository.GetByIdAsync(id);
              await _cacheService.SetAsync(cacheKey, user);
              return user;
          });
    }

    public async Task<ClientApp?> CreateAsync(ClientAppCreateModel model)
    {
      return await TraceAsync("CreateAsync", async () =>
          {
              var user = await _clientAppRepository.CreateAsync(model);
              if (!CacheEnabled)
              {
                  return user;
              }
              if (user != null)
              {
                  Scheduler.FireAndForget(() => InvalidateSearchCache());
              }
              return user;
          });
    }

    public async Task<ClientApp?> UpdateAsync(Guid id, ClientAppUpdateModel model)
    {
      return await TraceAsync("UpdateAsync", async () =>
          {
              var user = await _clientAppRepository.UpdateAsync(id, model);
              if (!CacheEnabled)
              {
                  return user;
              }
              if (user != null)
              {
                  Scheduler.FireAndForget(() => InvalidateUserCache(user));
              }
              return user;
          });
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
      return await TraceAsync("DeleteAsync", async () =>
          {
              var user = await _clientAppRepository.GetByIdAsync(id);
              if (user == null)
              {
                  return false;
              }
              var deleted = await _clientAppRepository.DeleteAsync(id);
              if (CacheEnabled && deleted)
              {
                  Scheduler.FireAndForget(() => InvalidateUserCache(user));
              }
              return deleted;
          });
    }

    public async Task<SearchResults<ClientApp>> SearchAsync(ClientAppSearchFilters filters)
    {
      return await TraceAsync("SearchAsync", async () =>
          {
              if (!CacheEnabled)
              {
                  return await _clientAppRepository.SearchAsync(filters);
              }
              var cacheKey = $"ClientAppService->SearchAsync->{JsonSerializer.Serialize<ClientAppSearchFilters>(filters)}";
              var cachedResults = await _cacheService.GetAsync<SearchResults<ClientApp>>(cacheKey);
              if (cachedResults != null)
              {
                  _logger.LogInformation("Retrieved client app search results from cache");
                  return cachedResults;
              }
              var results = await _clientAppRepository.SearchAsync(filters);
              await _cacheService.SetAsync(cacheKey, results);
              return results;
          });
    }

      public async Task<ClientApp?> GetByCodeAsync(string code)
      {
        return await TraceAsync("GetByUsernameAsync", async () =>
          {
              if (!CacheEnabled)
              {
                  return await _clientAppRepository.GetByCodeAsync(code);
              }
              var cacheKey = $"ClientAppService->GetByCodeAsync->code:{code}";
              var cachedUser = await _cacheService.GetAsync<ClientApp>(cacheKey);
              if (cachedUser != null)
              {
                  _logger.LogInformation("Retrieved client app from cache");
                  return cachedUser;
              }
              var user = await _clientAppRepository.GetByCodeAsync(code);
              await _cacheService.SetAsync(cacheKey, user);
              return user;
          });
      }

      public async Task<ClientApp?> GetByApiKeyAsync(string apiKey)
      {
        return await TraceAsync("GetByUsernameAsync", async () =>
          {
              if (!CacheEnabled)
              {
                  return await _clientAppRepository.GetByApiKeyAsync(apiKey);
              }
              var cacheKey = $"ClientAppService->GetByApiKeyAsync->apiKey:{apiKey}";
              var cachedUser = await _cacheService.GetAsync<ClientApp>(cacheKey);
              if (cachedUser != null)
              {
                  _logger.LogInformation("Retrieved client app from cache");
                  return cachedUser;
              }
              var user = await _clientAppRepository.GetByApiKeyAsync(apiKey);
              await _cacheService.SetAsync(cacheKey, user);
              return user;
          });
      }

      public void InvalidateUserCache(ClientApp? clientApp)
      {

          if (!CacheEnabled || clientApp == null)
          {
              return;
          }

          _logger.LogInformation("Invalidating all client app cache entries");

          _cacheService.FindAndClear<User>("ClientAppService->SearchAsync");

          var cacheKeys = new List<string>() {
                  $"ClientAppService->GetByIdAsync->id:{clientApp.Id}",
                  $"ClientAppService->GetByUsernameAsync->username:{clientApp.Name}",
              };
          _cacheService.Invalidate(cacheKeys);
      }

      public void InvalidateSearchCache()
      {
          if (!CacheEnabled)
          {
              return;
          }
          _logger.LogInformation("Invalidating all client app search cache entries");
          _cacheService.FindAndClear<User>("ClientAppService->SearchAsync");
      }

}
