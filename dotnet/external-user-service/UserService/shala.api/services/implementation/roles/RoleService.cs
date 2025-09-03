using System.Text.Json;
using shala.api.database.interfaces;
using shala.api.domain.types;
using shala.api.modules.cache;
using shala.api.startup;

namespace shala.api.services;

public class RoleService : BaseService<RoleService>, IRoleService
{

    #region Constructor

    private readonly IRoleRepository _roleRepository;

    public RoleService(IRoleRepository roleRepository,
                       ILogger<RoleService> logger,
                       IConfiguration configuration,
                       ICacheService cacheService)
        : base(configuration, logger, cacheService)
    {
        _roleRepository = roleRepository;
    }

    #endregion

    public async Task<Role?> CreateAsync(RoleCreateModel model)
    {
        return await TraceAsync("CreateAsync", async () =>
        {
            var user = await _roleRepository.CreateAsync(model);
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

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await TraceAsync("DeleteAsync", async () =>
        {
            var user = await _roleRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }
            var deleted = await _roleRepository.DeleteAsync(id);
            if (CacheEnabled && deleted)
            {
                Scheduler.FireAndForget(() => InvalidateUserCache(user));
            }
            return deleted;
        });
    }

    public async Task<Role?> GetByIdAsync(Guid id)
    {
        return await TraceAsync("GetByIdAsync", async () =>
        {
            if (!CacheEnabled)
            {
                return await _roleRepository.GetByIdAsync(id);
            }
            var cacheKey = $"RoleService->GetByIdAsync->id:{id}";
            var cachedUser = await _cacheService.GetAsync<Role>(cacheKey);
            if (cachedUser != null)
            {
                _logger.LogInformation("Retrieved role from cache");
                return cachedUser;
            }
            var user = await _roleRepository.GetByIdAsync(id);
            await _cacheService.SetAsync(cacheKey, user);
            return user;
        });
    }

    public async Task<Role?> GetByNameAsync(string name)
    {
        return await TraceAsync("GetByUsernameAsync", async () =>
        {
            if (!CacheEnabled)
            {
                return await _roleRepository.GetByNameAsync(name);
            }
            var cacheKey = $"RoleService->GetByNameAsync->name:{name}";
            var cachedUser = await _cacheService.GetAsync<Role>(cacheKey);
            if (cachedUser != null)
            {
                _logger.LogInformation("Retrieved role from cache");
                return cachedUser;
            }
            var user = await _roleRepository.GetByNameAsync(name);
            await _cacheService.SetAsync(cacheKey, user);
            return user;
        });
    }

    public async Task<SearchResults<Role>> SearchAsync(RoleSearchFilters filters)
    {
        return await TraceAsync("SearchAsync", async () =>
        {
            if (!CacheEnabled)
            {
                return await _roleRepository.SearchAsync(filters);
            }
            var cacheKey = $"RoleService->SearchAsync->{JsonSerializer.Serialize<RoleSearchFilters>(filters)}";
            var cachedResults = await _cacheService.GetAsync<SearchResults<Role>>(cacheKey);
            if (cachedResults != null)
            {
                _logger.LogInformation("Retrieved role search results from cache");
                return cachedResults;
            }
            var results = await _roleRepository.SearchAsync(filters);
            await _cacheService.SetAsync(cacheKey, results);
            return results;
        });
    }

    public async Task<Role?> UpdateAsync(Guid id, RoleUpdateModel model)
    {
        return await TraceAsync("UpdateAsync", async () =>
        {
            var user = await _roleRepository.UpdateAsync(id, model);
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

    public void InvalidateUserCache(Role? role)
    {

        if (!CacheEnabled || role == null)
        {
            return;
        }

        _logger.LogInformation("Invalidating all role cache entries");

        _cacheService.FindAndClear<User>("RoleService->SearchAsync");

        var cacheKeys = new List<string>() {
                $"RoleService->GetByIdAsync->id:{role.Id}",
                $"RoleService->GetByUsernameAsync->username:{role.Name}",
            };
        _cacheService.Invalidate(cacheKeys);
    }

    public void InvalidateSearchCache()
    {
        if (!CacheEnabled)
        {
            return;
        }
        _logger.LogInformation("Invalidating all role search cache entries");
        _cacheService.FindAndClear<Role>("RoleService->SearchAsync");
    }

}
