using shala.api.domain.types;

namespace shala.api.database.interfaces;

public interface IUserAuthProfileRepository
{
    Task<UserAuthProfile?> CreateUserAuthProfileAsync(Guid userId, string? passwordHash);
    Task<UserAuthProfile?> GetUserAuthProfileAsync(Guid userId);

    Task<bool> UpdateHashedPasswordAsync(Guid userId, string hashedPassword);
    Task<string?> GetHashedPasswordAsync(Guid userId);

    Task<bool> UpdateTotpSecretAsync(Guid userId, string secret);
    Task<string?> GetTotpSecretAsync(Guid userId);

    Task<bool> GetMfaEnabledAsync(Guid userId);
    Task<bool> SetMfaEnabledAsync(Guid userId, bool enabled);

    Task<string?> GetPreferredMfaTypeAsync(Guid userId);
    Task<bool> SetPreferredMfaTypeAsync(Guid userId, string mfaType);

    Task<bool> GetSignedUpWithOAuthAsync(Guid userId);
    Task<bool> SetSignedUpWithOAuthAsync(Guid userId, bool signedUpWithOAuth, string? provider);

    Task<bool> GetEmailVerifiedAsync(Guid userId);
    Task<bool> SetEmailVerifiedAsync(Guid userId, bool verified);

    Task<bool> GetPhoneVerifiedAsync(Guid userId);
    Task<bool> SetPhoneVerifiedAsync(Guid userId, bool verified);
}
