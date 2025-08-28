using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace AspNetJwtAuth.Demo
{
    /// <summary>
    /// Demo JWT generator for testing purposes
    /// This simulates an external authentication service
    /// </summary>
    public class DemoJwtGenerator
    {
        private readonly RSA _rsa;
        private readonly string _keyId;
        private readonly string _issuer;
        private readonly string _audience;

        public DemoJwtGenerator(string issuer = "https://demo-auth-service.com", string audience = "demo-api-service")
        {
            _rsa = RSA.Create(2048);
            _keyId = Guid.NewGuid().ToString();
            _issuer = issuer;
            _audience = audience;
        }

        /// <summary>
        /// Generate a JWT token for a user
        /// </summary>
        public string GenerateToken(string userId, string username, string[] roles, string[] permissions, TimeSpan? expiry = null)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("name", username)
            };

            // Add roles
            foreach (var role in roles)
            {
                claims.Add(new Claim("role", role));
            }

            // Add permissions
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permissions", permission));
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
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Generate JWKS (JSON Web Key Set) for token validation
        /// </summary>
        public string GenerateJwks()
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

            return JsonSerializer.Serialize(jwks, new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Get predefined user tokens for testing
        /// </summary>
        public static Dictionary<string, string> GetSampleTokens(DemoJwtGenerator generator)
        {
            var tokens = new Dictionary<string, string>();

            // Admin user
            tokens["admin"] = generator.GenerateToken(
                "1", 
                "admin", 
                new[] { "admin" }, 
                new[] { "read:posts", "write:posts", "delete:posts", "manage:users", "view:analytics" }
            );

            // Moderator user
            tokens["moderator"] = generator.GenerateToken(
                "2", 
                "moderator", 
                new[] { "moderator" }, 
                new[] { "read:posts", "write:posts", "delete:posts" }
            );

            // Regular user
            tokens["user"] = generator.GenerateToken(
                "3", 
                "user123", 
                new[] { "user" }, 
                new[] { "read:posts", "write:posts" }
            );

            // User with write permissions only
            tokens["writer"] = generator.GenerateToken(
                "4", 
                "writer", 
                new[] { "user" }, 
                new[] { "write:posts" }
            );

            // User with read permissions only
            tokens["reader"] = generator.GenerateToken(
                "5", 
                "reader", 
                new[] { "user" }, 
                new[] { "read:posts" }
            );

            return tokens;
        }

        public void Dispose()
        {
            _rsa?.Dispose();
        }
    }
}
