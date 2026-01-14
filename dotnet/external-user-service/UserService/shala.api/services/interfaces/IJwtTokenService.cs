using Microsoft.IdentityModel.Tokens;
using shala.api.domain.types;

namespace shala.api.services;

public interface IJwtTokenService
{
    /// <summary>
    /// Generate JWT token for authenticated user using asymmetric key
    /// </summary>
    string GenerateToken(User user, Guid sessionId, string? role);

    /// <summary>
    /// Generate JWKS (JSON Web Key Set) for token validation by other services
    /// </summary>
    string GenerateJwks();

    /// <summary>
    /// Get the current signing key for validation
    /// </summary>
    RsaSecurityKey GetSigningKey();

    /// <summary>
    /// Validate JWT token using asymmetric key
    /// </summary>
    UserVerificationResult ValidateToken(string token);
}
