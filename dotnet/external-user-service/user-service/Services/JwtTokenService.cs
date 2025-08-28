using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using AspNetJwtAuth.Models;
using AspNetJwtAuth.Services.Interfaces;

namespace AspNetJwtAuth.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly RSA _rsa;
        private readonly string _keyId;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly ILogger<JwtTokenService> _logger;

        public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
        {
            _rsa = RSA.Create(2048);
            
            // Make keyId persistent by using a hash of the RSA key parameters
            var parameters = _rsa.ExportParameters(false);
            var keyHash = Convert.ToBase64String(parameters.Modulus).Substring(0, 8);
            _keyId = keyHash;
            
            _issuer = configuration["Jwt:Issuer"] ?? "https://user-service.com";
            _audience = configuration["Jwt:Audience"] ?? "demo-api-service";
            _logger = logger;
        }

        /// <summary>
        /// Generate JWT token for authenticated user
        /// </summary>
        public string GenerateToken(User user, TimeSpan? expiry = null)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    new Claim("name", user.Username),
                    new Claim("email", user.Email ?? ""),
                    new Claim("firstName", user.FirstName ?? ""),
                    new Claim("lastName", user.LastName ?? "")
                };

                // Add roles
                foreach (var userRole in user.UserRoles)
                {
                    claims.Add(new Claim("role", userRole.Role.Name));
                }

                // Add permissions
                foreach (var userRole in user.UserRoles)
                {
                    foreach (var rolePermission in userRole.Role.RolePermissions)
                    {
                        claims.Add(new Claim("permissions", rolePermission.Permission.Name));
                    }
                }

                var tokenExpiry = expiry ?? TimeSpan.FromHours(1);
                var expires = DateTime.UtcNow.Add(tokenExpiry);

                var signingCredentials = new SigningCredentials(
                    new RsaSecurityKey(_rsa) { KeyId = _keyId },
                    SecurityAlgorithms.RsaSha256);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = expires,
                    Issuer = _issuer,
                    Audience = _audience,
                    SigningCredentials = signingCredentials
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                _logger.LogInformation("JWT token generated for user: {Username} with {RoleCount} roles and {PermissionCount} permissions", 
                    user.Username, user.UserRoles.Count, user.UserRoles.Sum(ur => ur.Role.RolePermissions.Count));

                return tokenString;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token for user: {Username}", user.Username);
                throw;
            }
        }

        /// <summary>
        /// Generate JWKS (JSON Web Key Set) for token validation by other services
        /// </summary>
        public string GenerateJwks()
        {
            try
            {
                var parameters = _rsa.ExportParameters(false);
                
                var jwk = new
                {
                    kty = "RSA",
                    use = "sig",
                    kid = _keyId,
                    alg = "RS256",
                    n = Base64UrlEncoder.Encode(parameters.Modulus),
                    e = Base64UrlEncoder.Encode(parameters.Exponent)
                };

                var jwks = new
                {
                    keys = new[] { jwk }
                };

                _logger.LogDebug("JWKS generated with key ID: {KeyId}", _keyId);
                return System.Text.Json.JsonSerializer.Serialize(jwks, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWKS");
                throw;
            }
        }

        /// <summary>
        /// Get the current signing key for validation
        /// </summary>
        public RsaSecurityKey GetSigningKey()
        {
            return new RsaSecurityKey(_rsa) { KeyId = _keyId };
        }
    }
}
