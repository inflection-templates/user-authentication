using shala.api.domain.types;

namespace shala.api.database.interfaces;

public interface ISessionRepository
{
    Task<Session?> CreateAsync(Guid userId);
    Task<Session?> CreateAsync(SessionCreateModel model);
    Task<Session?> GetByIdAsync(Guid sessionId);
    Task<Session?> UpdateMfaAuthenticatedAsync(Guid sessionId, bool mfaAuthenticated);
    Task<bool> LogoutAsync(Guid sessionId);
}
