using shala.api.domain.types;
using shala.api.database.interfaces;
using shala.api.modules.cache;
using System.Text.Json;
using shala.api.startup;

namespace shala.api.services;

public class UserService : BaseService<UserService>, IUserService
{

    #region Constructor

    private readonly IUserRepository _userRepository;

    public UserService(
        IConfiguration configuration,
        IUserRepository userRepository,
        ILogger<UserService> logger,
        ICacheService cacheService)
        : base(configuration, logger, cacheService)
    {
        _userRepository = userRepository;
    }

    #endregion

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await TraceAsync("GetByEmailAsync", async () =>
        {
            if (!CacheEnabled)
            {
                return await _userRepository.GetByEmailAsync(email);
            }
            // Return from cache if available
            var cacheKey = $"UserService->GetByEmailAsync->email:{email}";
            var cached = await _cacheService.GetAsync<User>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Retrieved user from cache");
                return cached;
            }
            var user = await _userRepository.GetByEmailAsync(email);
            await _cacheService.SetAsync(cacheKey, user);
            return user;
        });
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await TraceAsync("GetByIdAsync", async () =>
        {
            if (!CacheEnabled)
            {
                return await _userRepository.GetByIdAsync(id);
            }
            // Return from cache if available
            var cacheKey = $"UserService->GetByIdAsync->id:{id}";
            var cached = await _cacheService.GetAsync<User>(cacheKey);
            if (cached != null)
            {
                _logger.LogInformation("Retrieved user from cache");
                return cached;
            }
            var user = await _userRepository.GetByIdAsync(id);
            await _cacheService.SetAsync(cacheKey, user);
            return user;
        });
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await TraceAsync("GetByUsernameAsync", async () =>
        {
            if (!CacheEnabled)
            {
                return await _userRepository.GetByUserNameAsync(username);
            }
            var cacheKey = $"UserService->GetByUsernameAsync->username:{username}";
            var cachedUser = await _cacheService.GetAsync<User>(cacheKey);
            if (cachedUser != null)
            {
                _logger.LogInformation("Retrieved user from cache");
                return cachedUser;
            }
            var user = await _userRepository.GetByUserNameAsync(username);
            await _cacheService.SetAsync(cacheKey, user);
            return user;
        });
    }

    public async Task<User?> GetByPhoneAsync(string countryCode, string phoneNumber)
    {
        return await TraceAsync("GetByPhoneAsync", async () =>
        {
            if (!CacheEnabled)
            {
                return await _userRepository.GetByPhoneAsync(countryCode, phoneNumber);
            }
            var cacheKey = $"UserService->GetByPhoneAsync->countryCode:{countryCode}->phoneNumber:{phoneNumber}";
            var cachedUser = await _cacheService.GetAsync<User>(cacheKey);
            if (cachedUser != null)
            {
                _logger.LogInformation("Retrieved user from cache");
                return cachedUser;
            }
            var user = await _userRepository.GetByPhoneAsync(countryCode, phoneNumber);
            await _cacheService.SetAsync(cacheKey, user);
            return user;
        });
    }

    public async Task<User?> CreateAsync(UserCreateModel model)
    {
        return await TraceAsync("CreateAsync", async () =>
        {
            var user = await _userRepository.CreateAsync(model);
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

    public async Task<User?> UpdateAsync(Guid id, UserUpdateModel model)
    {
        return await TraceAsync("UpdateAsync", async () =>
        {
            var user = await _userRepository.UpdateAsync(id, model);
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
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }
            var deleted = await _userRepository.DeleteAsync(id);
            if (CacheEnabled && deleted)
            {
                Scheduler.FireAndForget(() => InvalidateUserCache(user));
            }
            return deleted;
        });
    }

    public async Task<SearchResults<User>> SearchAsync(UserSearchFilters filters)
    {
        return await TraceAsync("SearchAsync", async () =>
        {
            if (!CacheEnabled)
            {
                return await _userRepository.SearchAsync(filters);
            }
            var cacheKey = $"UserService->SearchAsync->{JsonSerializer.Serialize<UserSearchFilters>(filters)}";
            var cachedResults = await _cacheService.GetAsync<SearchResults<User>>(cacheKey);
            if (cachedResults != null)
            {
                _logger.LogInformation("Retrieved user search results from cache");
                return cachedResults;
            }
            var results = await _userRepository.SearchAsync(filters);
            await _cacheService.SetAsync(cacheKey, results);
            return results;
        });
    }

    public void InvalidateUserCache(User? user)
    {

        if (!CacheEnabled || user == null)
        {
            return;
        }

        _logger.LogInformation("Invalidating all user cache entries");

        // Clear the search cache: Bit high handed, but we can't be sure
        // if the user was part of the search results
        _cacheService.FindAndClear<User>("UserService->SearchAsync");

        var cacheKeys = new List<string>() {
                $"UserService->GetByIdAsync->id:{user.Id}",
                $"UserService->GetByUsernameAsync->username:{user.UserName}",
                $"UserService->GetByEmailAsync->email:{user.Email}",
                $"UserService->GetByPhoneAsync->countryCode:{user.CountryCode}->phoneNumber:{user.PhoneNumber}"
            };
        _cacheService.Invalidate(cacheKeys);
    }

    public void InvalidateSearchCache()
    {
        if (!CacheEnabled)
        {
            return;
        }
        _logger.LogInformation("Invalidating all user search cache entries");
        _cacheService.FindAndClear<User>("UserService->SearchAsync");
    }

}

