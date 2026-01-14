using System.Text.Json;
using shala.api.database.interfaces;
using shala.api.domain.types;
using shala.api.modules.cache;
using Microsoft.Extensions.Logging;
using shala.api.startup;

namespace shala.api.services;

public class UserAuthProfileService : BaseService<UserAuthProfileService>, IUserAuthProfileService
{
    #region Constructor

    private readonly IUserRepository _userRepository;
    private readonly IUserAuthProfileRepository _userAuthProfileRepository;

    public UserAuthProfileService(
        IUserRepository userRepository,
        IUserAuthProfileRepository userAuthProfileRepository,
        ILogger<UserAuthProfileService> logger,
        IConfiguration configuration,
        ICacheService cacheService)
        : base(configuration, logger, cacheService)
    {
        _userRepository = userRepository;
        _userAuthProfileRepository = userAuthProfileRepository;
    }

    #endregion

    public async Task<UserAuthProfile?> GetUserAuthProfileAsync(Guid userId)
    {
        return await TraceAsync("GetUserAuthProfileAsync", async () =>
        {
            if (!CacheEnabled)
            {
                return await _userAuthProfileRepository.GetUserAuthProfileAsync(userId);
            }
            var cacheKey = $"UserAuthProfileService->GetUserAuthProfileAsync->userId:{userId}";
            var cachedProfile = await _cacheService.GetAsync<UserAuthProfile>(cacheKey);
            if (cachedProfile != null)
            {
                _logger.LogInformation("Retrieved user auth profile from cache");
                return cachedProfile;
            }
            var profile = await _userAuthProfileRepository.GetUserAuthProfileAsync(userId);
            await _cacheService.SetAsync(cacheKey, profile);
            return profile;
        });
    }

    public async Task<UserAuthProfile?> CreateUserAuthProfileAsync(Guid userId, string? passwordHash)
    {
        return await TraceAsync("CreateUserAuthProfileAsync", async () =>
        {
            var profile = await _userAuthProfileRepository.CreateUserAuthProfileAsync(userId, passwordHash);
            if (CacheEnabled && profile != null)
            {
                Scheduler.FireAndForget(() => InvalidateUserCache(userId));
            }
            return profile;
        });
    }

    public async Task<bool> UpdateHashedPasswordAsync(Guid userId, string hashedPassword)
    {
        return await TraceAsync("UpdateHashedPasswordAsync", async () =>
        {
            var updated = await _userAuthProfileRepository.UpdateHashedPasswordAsync(userId, hashedPassword);
            if (CacheEnabled && updated)
            {
                Scheduler.FireAndForget(() => InvalidateUserCache(userId));
            }
            return updated;
        });
    }

    public async Task<string?> GetHashedPasswordAsync(Guid userId)
    {
        return await TraceAsync("GetHashedPasswordAsync", async () =>
        {
            return await _userAuthProfileRepository.GetHashedPasswordAsync(userId);
        });
    }

    public async Task<bool> UpdateTotpSecretAsync(Guid userId, string secret)
    {
        return await TraceAsync("UpdateTotpSecretAsync", async () =>
        {
            var updated = await _userAuthProfileRepository.UpdateTotpSecretAsync(userId, secret);
            if (CacheEnabled && updated)
            {
                Scheduler.FireAndForget(() => InvalidateUserCache(userId));
            }
            return updated;
        });
    }

    public async Task<string?> GetTotpSecretAsync(Guid userId)
    {
        return await TraceAsync("GetTotpSecretAsync", async () =>
        {
            return await _userAuthProfileRepository.GetTotpSecretAsync(userId);
        });
    }

    public async Task<bool> GetMfaEnabledAsync(Guid userId)
    {
        return await TraceAsync("GetMfaEnabledAsync", async () =>
        {
            return await _userAuthProfileRepository.GetMfaEnabledAsync(userId);
        });
    }

    public async Task<bool> SetMfaEnabledAsync(Guid userId, bool enabled)
    {
        return await TraceAsync("SetMfaEnabledAsync", async () =>
        {
            var updated = await _userAuthProfileRepository.SetMfaEnabledAsync(userId, enabled);
            if (CacheEnabled && updated)
            {
                Scheduler.FireAndForget(() => InvalidateUserCache(userId));
            }
            return updated;
        });
    }

    public async Task<string?> GetPreferredMfaTypeAsync(Guid userId)
    {
        return await TraceAsync("GetPreferredMfaTypeAsync", async () =>
        {
            return await _userAuthProfileRepository.GetPreferredMfaTypeAsync(userId);
        });
    }

    public async Task<bool> SetPreferredMfaTypeAsync(Guid userId, string mfaType)
    {
        return await TraceAsync("SetPreferredMfaTypeAsync", async () =>
        {
            var updated = await _userAuthProfileRepository.SetPreferredMfaTypeAsync(userId, mfaType);
            if (CacheEnabled && updated)
            {
                Scheduler.FireAndForget(() => InvalidateUserCache(userId));
            }
            return updated;
        });
    }

    public async Task<bool> GetSignedUpWithOAuthAsync(Guid userId)
    {
        return await TraceAsync("GetSignedUpWithOAuthAsync", async () =>
        {
            return await _userAuthProfileRepository.GetSignedUpWithOAuthAsync(userId);
        });
    }

    public async Task<bool> SetSignedUpWithOAuthAsync(Guid userId, bool signedUpWithOAuth, string? provider)
    {
        return await TraceAsync("SetSignedUpWithOAuthAsync", async () =>
        {
            var updated = await _userAuthProfileRepository.SetSignedUpWithOAuthAsync(userId, signedUpWithOAuth, provider);
            if (CacheEnabled && updated)
            {
                Scheduler.FireAndForget(() => InvalidateUserCache(userId));
            }
            return updated;
        });
    }

    public async Task<bool> GetEmailVerifiedAsync(Guid userId)
    {
        return await TraceAsync("GetEmailVerifiedAsync", async () =>
        {
            return await _userAuthProfileRepository.GetEmailVerifiedAsync(userId);
        });
    }

    public async Task<bool> SetEmailVerifiedAsync(Guid userId, bool verified)
    {
        return await TraceAsync("SetEmailVerifiedAsync", async () =>
        {
            var updated = await _userAuthProfileRepository.SetEmailVerifiedAsync(userId, verified);
            if (CacheEnabled && updated)
            {
                Scheduler.FireAndForget(() => InvalidateUserCache(userId));
            }
            return updated;
        });
    }

    public async Task<bool> GetPhoneVerifiedAsync(Guid userId)
    {
        return await TraceAsync("GetPhoneVerifiedAsync", async () =>
        {
            return await _userAuthProfileRepository.GetPhoneVerifiedAsync(userId);
        });
    }

    public async Task<bool> SetPhoneVerifiedAsync(Guid userId, bool verified)
    {
        return await TraceAsync("SetPhoneVerifiedAsync", async () =>
        {
            var updated = await _userAuthProfileRepository.SetPhoneVerifiedAsync(userId, verified);
            if (CacheEnabled && updated)
            {
                Scheduler.FireAndForget(() => InvalidateUserCache(userId));
            }
            return updated;
        });
    }

    public void InvalidateUserCache(Guid userId)
    {
        if (!CacheEnabled)
        {
            return;
        }

        _logger.LogInformation("Invalidating user auth profile cache entries");

        var cacheKeys = new List<string>
        {
            $"UserAuthProfileService->GetUserAuthProfileAsync->userId:{userId}",
            $"UserAuthProfileService->GetHashedPasswordAsync->userId:{userId}",
            $"UserAuthProfileService->GetTotpSecretAsync->userId:{userId}",
            $"UserAuthProfileService->GetPreferredMfaTypeAsync->userId:{userId}"
        };
        _cacheService.Invalidate(cacheKeys);
    }
}





// using shala.api.database.interfaces;
// using shala.api.domain.types;

// namespace shala.api.services;

// public class UserAuthProfileService : IUserAuthProfileService
// {

//     #region Constructor

//     private readonly IUserRepository _userRepository;
//     private readonly IUserAuthProfileRepository _userAuthProfileRepository;
//     private readonly ILogger<UserAuthProfileService> _logger;

//     public UserAuthProfileService(
//         IUserRepository userRepository,
//         IUserAuthProfileRepository userAuthProfileRepository,
//         ILogger<UserAuthProfileService> logger)
//     {
//         _userRepository = userRepository;
//         _userAuthProfileRepository = userAuthProfileRepository;
//         _logger = logger;
//     }

//     #endregion

//     public async Task<UserAuthProfile?> GetUserAuthProfileAsync(Guid userId)
//     {
//         return await _userAuthProfileRepository.GetUserAuthProfileAsync(userId);
//     }

//     public async Task<UserAuthProfile?> CreateUserAuthProfileAsync(Guid userId, string? passwordHash)
//     {
//         return await _userAuthProfileRepository.CreateUserAuthProfileAsync(userId, passwordHash);
//     }

//     public async Task<bool> UpdateHashedPasswordAsync(Guid userId, string hashedPassword)
//     {
//         return await _userAuthProfileRepository.UpdateHashedPasswordAsync(userId, hashedPassword);
//     }

//     public async Task<string?> GetHashedPasswordAsync(Guid userId)
//     {
//         return await _userAuthProfileRepository.GetHashedPasswordAsync(userId);
//     }

//     public async Task<bool> UpdateTotpSecretAsync(Guid userId, string secret)
//     {
//         return await _userAuthProfileRepository.UpdateTotpSecretAsync(userId, secret);
//     }

//     public async Task<string?> GetTotpSecretAsync(Guid userId)
//     {
//         return await _userAuthProfileRepository.GetTotpSecretAsync(userId);
//     }

//     public async Task<bool> GetMfaEnabledAsync(Guid userId)
//     {
//         return await _userAuthProfileRepository.GetMfaEnabledAsync(userId);
//     }

//     public async Task<bool> SetMfaEnabledAsync(Guid userId, bool enabled)
//     {
//         return await _userAuthProfileRepository.SetMfaEnabledAsync(userId, enabled);
//     }

//     public Task<string?> GetPreferredMfaTypeAsync(Guid userId)
//     {
//         return _userAuthProfileRepository.GetPreferredMfaTypeAsync(userId);
//     }

//     public Task<bool> SetPreferredMfaTypeAsync(Guid userId, string mfaType)
//     {
//         return _userAuthProfileRepository.SetPreferredMfaTypeAsync(userId, mfaType);
//     }

//     public async Task<bool> GetSignedUpWithOAuthAsync(Guid userId)
//     {
//         return await _userAuthProfileRepository.GetSignedUpWithOAuthAsync(userId);
//     }

//     public async Task<bool> SetSignedUpWithOAuthAsync(Guid userId, bool signedUpWithOAuth, string? provider)
//     {
//         return await _userAuthProfileRepository.SetSignedUpWithOAuthAsync(userId, signedUpWithOAuth, provider);
//     }

//     public async Task<bool> GetEmailVerifiedAsync(Guid userId)
//     {
//         return await _userAuthProfileRepository.GetEmailVerifiedAsync(userId);
//     }

//     public async Task<bool> SetEmailVerifiedAsync(Guid userId, bool verified)
//     {
//         return await _userAuthProfileRepository.SetEmailVerifiedAsync(userId, verified);
//     }

//     public async Task<bool> GetPhoneVerifiedAsync(Guid userId)
//     {
//         return await _userAuthProfileRepository.GetPhoneVerifiedAsync(userId);
//     }

//     public async Task<bool> SetPhoneVerifiedAsync(Guid userId, bool verified)
//     {
//         return await _userAuthProfileRepository.SetPhoneVerifiedAsync(userId, verified);
//     }

// }
