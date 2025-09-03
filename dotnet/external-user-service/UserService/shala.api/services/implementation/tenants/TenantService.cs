using System.Text.Json;
using shala.api.database.interfaces;
using shala.api.domain.types;
using shala.api.modules.cache;
using shala.api.startup;

namespace shala.api.services;

public class TenantService : BaseService<TenantService>, ITenantService
{

    #region Constructor

    private readonly ITenantRepository _tenantRepository;
    // private readonly ILogger<TenantService> _logger;

    public TenantService(
        IConfiguration configuration,
        ITenantRepository tenantRepository,
        ILogger<TenantService> logger,
        ICacheService cacheService)
        : base(configuration, logger, cacheService)
    {
        _tenantRepository = tenantRepository;
    }

    // public TenantService(ITenantRepository tenantRepository, ILogger<TenantService> logger)
    // {
    //   _tenantRepository = tenantRepository;
    //   _logger = logger;
    // }

    #endregion

    public async Task<Tenant?> GetByIdAsync(Guid id)
    {
      return await TraceAsync("GetByIdAsync", async () =>
        {
            if (!CacheEnabled)
            {
                return await _tenantRepository.GetByIdAsync(id);
            }
            // Return from cache if available
            var cacheKey = $"TenantService->GetByIdAsync->id:{id}";
            var cachedUser = await _cacheService.GetAsync<Tenant>(cacheKey);
            if (cachedUser != null)
            {
                _logger.LogInformation("Retrieved tenant from cache");
                return cachedUser;
            }
            var user = await _tenantRepository.GetByIdAsync(id);
            await _cacheService.SetAsync(cacheKey, user);
            return user;
        });
      // return await _tenantRepository.GetByIdAsync(id);
    }

    public async Task<Tenant?> CreateAsync(TenantCreateModel model)
    {
        return await TraceAsync("CreateAsync", async () =>
        {
            var user = await _tenantRepository.CreateAsync(model);
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
      // return await _tenantRepository.CreateAsync(model);
    }

    public async Task<Tenant?> UpdateAsync(Guid id, TenantUpdateModel model)
    {
        return await TraceAsync("UpdateAsync", async () =>
        {
            var user = await _tenantRepository.UpdateAsync(id, model);
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
      // var tenant = await _tenantRepository.GetByIdAsync(id);
      // if (tenant == null)
      // {
      //   throw new Exception("Tenant not found");
      // }
      // return await _tenantRepository.UpdateAsync(id, model);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await TraceAsync("DeleteAsync", async () =>
        {
            var user = await _tenantRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }
            var deleted = await _tenantRepository.DeleteAsync(id);
            if (CacheEnabled && deleted)
            {
                Scheduler.FireAndForget(() => InvalidateUserCache(user));
            }
            return deleted;
        });
      // var tenant = await _tenantRepository.GetByIdAsync(id);
      // if (tenant == null)
      // {
      //   throw new Exception("Tenant not found");
      // }
      // return await _tenantRepository.DeleteAsync(id);
    }

    public async Task<SearchResults<Tenant>> SearchAsync(TenantSearchFilters filters)
    {
        return await TraceAsync("SearchAsync", async () =>
        {
            if (!CacheEnabled)
            {
                return await _tenantRepository.SearchAsync(filters);
            }
            var cacheKey = $"UserService->SearchAsync->{JsonSerializer.Serialize<TenantSearchFilters>(filters)}";
            var cachedResults = await _cacheService.GetAsync<SearchResults<Tenant>>(cacheKey);
            if (cachedResults != null)
            {
                _logger.LogInformation("Retrieved tenant search results from cache");
                return cachedResults;
            }
            var results = await _tenantRepository.SearchAsync(filters);
            await _cacheService.SetAsync(cacheKey, results);
            return results;
        });
      // var results = await _tenantRepository.SearchAsync(filters);
      // return results;
    }

      public async Task<Tenant?> GetByCodeAsync(string code)
      {
          return await TraceAsync("GetByCodeAsync", async () =>
        {
            if (!CacheEnabled)
            {
                return await _tenantRepository.GetByCodeAsync(code);
            }
            var cacheKey = $"UserService->GetByCodeAsync->code:{code}";
            var cachedUser = await _cacheService.GetAsync<Tenant>(cacheKey);
            if (cachedUser != null)
            {
                _logger.LogInformation("Retrieved tenant from cache");
                return cachedUser;
            }
            var user = await _tenantRepository.GetByCodeAsync(code);
            await _cacheService.SetAsync(cacheKey, user);
            return user;
        });
          // return await _tenantRepository.GetByCodeAsync(code);
      }

      public async Task<Tenant?> GetByEmailAsync(string email)
      {
          return await TraceAsync("GetByEmailAsync", async () =>
        {
            if (!CacheEnabled)
            {
                return await _tenantRepository.GetByEmailAsync(email);
            }
            // Return from cache if available
            var cacheKey = $"UserService->GetByEmailAsync->email:{email}";
            var cachedUser = await _cacheService.GetAsync<Tenant>(cacheKey);
            if (cachedUser != null)
            {
                _logger.LogInformation("Retrieved user from cache");
                return cachedUser;
            }
            var user = await _tenantRepository.GetByEmailAsync(email);
            await _cacheService.SetAsync(cacheKey, user);
            return user;
        });
          // return await _tenantRepository.GetByEmailAsync(email);
      }

      public void InvalidateUserCache(Tenant? tenant)
      {

          if (!CacheEnabled || tenant == null)
          {
              return;
          }

          _logger.LogInformation("Invalidating all tenant cache entries");

          // Clear the search cache: Bit high handed, but we can't be sure
          // if the user was part of the search results
          _cacheService.FindAndClear<User>("TenantService->SearchAsync");

          var cacheKeys = new List<string>() {
                  $"TenantService->GetByIdAsync->id:{tenant.Id}",
                  $"TenantService->GetByUsernameAsync->username:{tenant.Name}",
              };
          _cacheService.Invalidate(cacheKeys);
      }
      
      public void InvalidateSearchCache()
      {
          if (!CacheEnabled)
          {
              return;
          }
          _logger.LogInformation("Invalidating all tenant search cache entries");
          _cacheService.FindAndClear<User>("TenantService->SearchAsync");
      }


}
