using Microsoft.IdentityModel.Tokens;
using AspNetJwtAuth.Models;

namespace AspNetJwtAuth.Services.Interfaces
{
    public interface IJwtTokenService
    {
        /// <summary>
        /// Generate JWT token for authenticated user
        /// </summary>
        string GenerateToken(User user, TimeSpan? expiry = null);

        /// <summary>
        /// Generate JWKS (JSON Web Key Set) for token validation by other services
        /// </summary>
        string GenerateJwks();

        /// <summary>
        /// Get the current signing key for validation
        /// </summary>
        RsaSecurityKey GetSigningKey();
    }
}
