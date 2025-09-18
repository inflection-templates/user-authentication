using System.Text.Json;
using shala.api.database.interfaces;
using shala.api.domain.types;
using shala.api.modules.cache;
using shala.api.startup;

namespace shala.api.services;

public class UserAuthService : BaseService<UserAuthService>, IUserAuthService
{

    #region Constructor

    private readonly IUserRepository _userRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IOtpRepository _otpRepository;

    public UserAuthService(
        IUserRepository userRepository,
        ISessionRepository sessionRepository,
        IOtpRepository otpRepository,
        IUserAuthProfileRepository userAuthProfileRepository,
        ILogger<UserAuthService> logger,
        IConfiguration configuration,
        ICacheService cacheService)
        : base(configuration, logger, cacheService)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _otpRepository = otpRepository;
    }

    #endregion

    public async Task<Session?> CreateSessionAsync(Guid userId)
    {
        return await TraceAsync("CreateSessionAsync", async () =>
        {
            var session = await _sessionRepository.CreateAsync(userId);
            if (!CacheEnabled)
            {
                return session;
            }
            if (session != null)
            {
                Scheduler.FireAndForget(() => InvalidateSessionCache(userId));
            }
            return session;
        });
    }

    public async Task<Session?> CreateSessionAsync(SessionCreateModel model)
    {
        return await TraceAsync("CreateSessionAsync(Model)", async () =>
        {
            var session = await _sessionRepository.CreateAsync(model);
            if (!CacheEnabled)
            {
                return session;
            }
            if (session != null)
            {
                Scheduler.FireAndForget(() => InvalidateSessionCache(model.UserId));
            }
            return session;
        });
    }

    public async Task<Session?> GetSessionAsync(Guid sessionId)
    {
        return await TraceAsync("GetSessionAsync", async () =>
        {
            if (!CacheEnabled)
            {
                return await _sessionRepository.GetByIdAsync(sessionId);
            }
            var cacheKey = $"UserAuthService->GetSessionAsync->sessionId:{sessionId}";
            var cachedSession = await _cacheService.GetAsync<Session>(cacheKey);
            if (cachedSession != null)
            {
                _logger.LogInformation("Retrieved session from cache");
                return cachedSession;
            }
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            await _cacheService.SetAsync(cacheKey, session);
            return session;
        });
    }

    public async Task<Session?> UpdateSessionMfaAuthenticatedAsync(Guid sessionId, bool mfaAuthenticated)
    {
        return await TraceAsync("UpdateSessionMfaAuthenticatedAsync", async () =>
        {
            var session = await _sessionRepository.UpdateMfaAuthenticatedAsync(sessionId, mfaAuthenticated);
            if (CacheEnabled && session != null)
            {
                Scheduler.FireAndForget(() => InvalidateSessionCache(session.UserId));
            }
            return session;
        });
    }

    public async Task<bool> LogoutSessionAsync(Guid sessionId)
    {
        return await TraceAsync("LogoutSessionAsync", async () =>
        {
            var session = await _sessionRepository.GetByIdAsync(sessionId);
            if (session == null)
            {
                return false;
            }
            var loggedOut = await _sessionRepository.LogoutAsync(sessionId);
            if (CacheEnabled && loggedOut)
            {
                Scheduler.FireAndForget(() => InvalidateSessionCache(session.UserId));
            }
            return loggedOut;
        });
    }

    public async Task<Otp?> CreateOtpAsync(Guid userId, string otp, string purpose)
    {
        return await TraceAsync("CreateOtpAsync", async () =>
        {
            var otpResult = await _otpRepository.CreateAsync(userId, otp, purpose);
            if (CacheEnabled && otpResult != null)
            {
                Scheduler.FireAndForget(() => InvalidateOtpCache(userId));
            }
            return otpResult;
        });
    }

    public async Task<Otp?> GetOtpAsync(Guid userId, string otp)
    {
        return await TraceAsync("GetOtpAsync", async () =>
        {
            if (!CacheEnabled)
            {
                return await _otpRepository.GetByOtpAsync(userId, otp);
            }
            var cacheKey = $"UserAuthService->GetOtpAsync->userId:{userId}->otp:{otp}";
            var cachedOtp = await _cacheService.GetAsync<Otp>(cacheKey);
            if (cachedOtp != null)
            {
                _logger.LogInformation("Retrieved OTP from cache");
                return cachedOtp;
            }
            var otpResult = await _otpRepository.GetByOtpAsync(userId, otp);
            await _cacheService.SetAsync(cacheKey, otpResult);
            return otpResult;
        });
    }

    public void InvalidateSessionCache(Guid userId)
    {
        if (!CacheEnabled)
        {
            return;
        }

        _logger.LogInformation("Invalidating session cache entries for user");
        _cacheService.FindAndClear<Session>($"UserAuthService->GetSessionAsync->userId:{userId}");
    }

    public void InvalidateOtpCache(Guid userId)
    {
        if (!CacheEnabled)
        {
            return;
        }

        _logger.LogInformation("Invalidating OTP cache entries for user");
        _cacheService.FindAndClear<Otp>($"UserAuthService->GetOtpAsync->userId:{userId}");
    }
}




// using shala.api.database.interfaces;
// using shala.api.domain.types;

// namespace shala.api.services;

// public class UserAuthService : IUserAuthService
// {

//     #region Constructor

//     private readonly IUserRepository _userRepository;
//     private readonly ISessionRepository _sessionRepository;
//     private readonly IOtpRepository _otpRepository;
//     private readonly ILogger<UserAuthService> _logger;

//     public UserAuthService(
//         IUserRepository userRepository,
//         ISessionRepository sessionRepository,
//         IOtpRepository otpRepository,
//         IUserAuthProfileRepository userAuthProfileRepository,
//         ILogger<UserAuthService> logger)
//     {
//         _userRepository = userRepository;
//         _sessionRepository = sessionRepository;
//         _otpRepository = otpRepository;
//         _logger = logger;
//     }

//     #endregion

//     public async Task<Session?> CreateSessionAsync(Guid userId)
//     {
//         return await _sessionRepository.CreateAsync(userId);
//     }

//     public async Task<Session?> CreateSessionAsync(SessionCreateModel model)
//     {
//         return await _sessionRepository.CreateAsync(model);
//     }

//     public async Task<Session?> GetSessionAsync(Guid sessionId)
//     {
//         return await _sessionRepository.GetByIdAsync(sessionId);
//     }

//     public Task<Session?> UpdateSessionMfaAuthenticatedAsync(Guid sessionId, bool mfaAuthenticated)
//     {
//         return _sessionRepository.UpdateMfaAuthenticatedAsync(sessionId, mfaAuthenticated);
//     }

//     public async Task<bool> LogoutSessionAsync(Guid sessionId)
//     {
//         return await _sessionRepository.LogoutAsync(sessionId);
//     }

//     public async Task<Otp?> CreateOtpAsync(Guid userId, string otp, string purpose)
//     {
//         return await _otpRepository.CreateAsync(userId, otp, purpose);
//     }

//     public async Task<Otp?> GetOtpAsync(Guid userId, string otp)
//     {
//         return await _otpRepository.GetByOtpAsync(userId, otp);
//     }

// }
