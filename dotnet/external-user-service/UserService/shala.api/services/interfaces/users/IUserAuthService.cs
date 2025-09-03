using shala.api.domain.types;

namespace shala.api.services;

public interface IUserAuthService
{
    Task<Otp?> GetOtpAsync(Guid userId, string otp);
    Task<Otp?> CreateOtpAsync(Guid userId, string otp, string purpose);

    Task<Session?> CreateSessionAsync(Guid userId);
    Task<Session?> CreateSessionAsync(SessionCreateModel session);
    Task<Session?> GetSessionAsync(Guid sessionId);
    Task<Session?> UpdateSessionMfaAuthenticatedAsync(Guid sessionId, bool mfaAuthenticated);
    Task<bool> LogoutSessionAsync(Guid sessionId);
}
