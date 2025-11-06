using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using shala.api.domain.types;
using System.Text.Json;

namespace shala.api.services;

public class JwtTokenService : IJwtTokenService
{
    private readonly RSA _rsa;
    private readonly string _keyId;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly ILogger<JwtTokenService> _logger;

    public JwtTokenService(IConfiguration configuration, ILogger<JwtTokenService> logger)
    {
        _logger = logger;  // Set logger first
        
        _rsa = GetOrCreatePersistentRsaKey(logger);
        
        // Make keyId persistent by using a hash of the RSA key parameters
        var parameters = _rsa.ExportParameters(false);
        var keyHash = Convert.ToBase64String(parameters.Modulus ?? Array.Empty<byte>()).Substring(0, 8);
        _keyId = keyHash;
        
        _issuer = configuration["Secrets:Jwt:Issuer"] ?? "shala";
        _audience = configuration["Secrets:Jwt:Audience"] ?? "shala";
        
        _logger.LogInformation("JWT Token Service initialized with persistent key ID: {KeyId}", _keyId);
    }

    /// <summary>
    /// Generate JWT token for authenticated user using asymmetric key
    /// </summary>
    public string GenerateToken(User user, Guid sessionId, string? role)
    {
        try
        {
            var claims = getUserClaims(user, role, sessionId);
            
            var signingCredentials = new SigningCredentials(
                new RsaSecurityKey(_rsa) { KeyId = _keyId },
                SecurityAlgorithms.RsaSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(5), // 5 days validity
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = signingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation("JWT token generated for user: {Username} with role: {Role}", 
                user.UserName, role ?? "No role");

            return tokenString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating JWT token for user: {Username}", user.UserName);
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
            return JsonSerializer.Serialize(jwks, new JsonSerializerOptions { WriteIndented = true });
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

    /// <summary>
    /// Validate JWT token using asymmetric key
    /// </summary>
    public UserVerificationResult ValidateToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = GetSigningKey(),
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = true,
                ValidAudience = _audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var claimsPrincipal = handler.ValidateToken(token, validationParameters, out var validatedToken);
            var userIdStr = claimsPrincipal.FindFirst(ClaimTypes.Sid)?.Value;
            var userName = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var sessionIdStr = claimsPrincipal.FindFirst("SessionId")?.Value;
            
            if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(sessionIdStr))
            {
                throw new Exception("Invalid token.");
            }
            
            var userId = Guid.Parse(userIdStr);
            var sessionId = Guid.Parse(sessionIdStr);
            
            return new UserVerificationResult
            {
                UserId = userId,
                UserName = userName ?? "Unspecified",
                Role = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value ?? "Unspecified",
                SessionId = sessionId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating JWT token");
            throw;
        }
    }

    /// <summary>
    /// Get user claims for JWT token
    /// </summary>
    private List<Claim> getUserClaims(User user, string? role, Guid sessionId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Sid, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.UserName ?? ""),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.GivenName, user.FirstName ?? ""),
            new Claim(ClaimTypes.Surname, user.LastName ?? ""),
            new Claim("SessionId", sessionId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        if (!string.IsNullOrEmpty(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return claims;
    }

    /// <summary>
    /// Get or create persistent RSA key that survives service restarts
    /// </summary>
    private RSA GetOrCreatePersistentRsaKey(ILogger<JwtTokenService> logger)
    {
        var keyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "jwt-rsa-key.xml");
        
        try
        {
            // Try to load existing key
            if (File.Exists(keyFilePath))
            {
                var keyXml = File.ReadAllText(keyFilePath);
                var rsa = RSA.Create();
                rsa.FromXmlString(keyXml);
                _logger.LogInformation("Loaded existing persistent RSA key from: {KeyPath}", keyFilePath);
                return rsa;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load existing RSA key, will create new one");
        }

        // Create new key if none exists
        var newRsa = RSA.Create(2048);
        
        try
        {
            // Save the new key for future use
            var keyXml = newRsa.ToXmlString(true); // true = include private key
            File.WriteAllText(keyFilePath, keyXml);
            _logger.LogInformation("Created and saved new persistent RSA key to: {KeyPath}", keyFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to save RSA key, but continuing with in-memory key");
        }
        
        return newRsa;
    }
}
