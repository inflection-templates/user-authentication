using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using shala.api.domain.types;

namespace shala.api.startup;

public static class UserAuthenticationHandler
{

    public static JwtSecurityToken GenerateAuthToken(IConfiguration configuration, User user, Guid sessionId, string? role)
    {

        var jwtAuthSecret = configuration.GetValue<string>("Secrets:Jwt:UserAccessTokenSecret");
        if (string.IsNullOrEmpty(jwtAuthSecret))
        {
            throw new ArgumentNullException("UserAccessTokenSecret is missing in appsettings.json");
        }

        var issuer = configuration.GetValue<string>("Secrets:Jwt:Issuer");
        if (string.IsNullOrEmpty(issuer))
        {
            issuer = "shala.api";
        }
        var audience = configuration.GetValue<string>("Secrets:Jwt:Audience");
        if (string.IsNullOrEmpty(audience))
        {
            audience = "shala.api";
        }
        var validityInDays = configuration.GetValue<int>("Secrets:Jwt:AccessTokenValidityInDays");
        if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
        {
            validityInDays = 5;
        }

        var claims = getUserClaims(user, role, sessionId);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthSecret));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            expires: DateTime.UtcNow.AddDays(validityInDays),
            claims: claims.ToArray(),
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            );

        return token;
    }

    public static UserVerificationResult ValidateAuthToken(IConfiguration configuration, string token)
    {
        var secret = configuration.GetValue<string>("Secrets:Jwt:UserAccessTokenSecret");
        if (string.IsNullOrEmpty(secret))
        {
            throw new ArgumentNullException("UserAccessTokenSecret is missing in appsettings.json");
        }

        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secret);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
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

    public static JwtSecurityToken GenerateRefreshToken(IConfiguration configuration, User user, string? role)
    {

        var jwtAuthSecret = configuration.GetValue<string>("Secrets:Jwt:UserRefreshTokenSecret");
        if (string.IsNullOrEmpty(jwtAuthSecret))
        {
            throw new ArgumentNullException("UserRefreshTokenSecret is missing in appsettings.json");
        }

        var issuer = configuration.GetValue<string>("Secrets:Jwt:Issuer");
        if (string.IsNullOrEmpty(issuer))
        {
            issuer = "shala.api";
        }
        var audience = configuration.GetValue<string>("Secrets:Jwt:Audience");
        if (string.IsNullOrEmpty(audience))
        {
            audience = "shala.api";
        }
        var validityInDays = configuration.GetValue<int>("Secrets:Jwt:RefreshTokenValidityInDays");
        if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
        {
            validityInDays = 365;
        }

        List<Claim> claims = getUserClaims(user, role);
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthSecret));

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            expires: DateTime.UtcNow.AddDays(validityInDays),
            claims: claims.ToArray(),
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            );

        return token;
    }

    private static List<Claim> getUserClaims(User user, string? role, Guid? sessionId = null)
    {
        var claims = new List<Claim>();

        string phone = user.CountryCode + "-" + user.PhoneNumber;

        claims.Add(new Claim(ClaimTypes.Sid, user.Id.ToString(), ClaimValueTypes.UInteger32));
        if (!string.IsNullOrEmpty(user.Email))
        {
            claims.Add(new Claim(ClaimTypes.Email, user.Email, ClaimValueTypes.Email));
        }
        if (user.TenantId != null && user.TenantId != Guid.Empty)
        {
            var tenantId = user.TenantId ?? Guid.Empty;
            claims.Add(new Claim("TenantId", tenantId.ToString(), ClaimValueTypes.String));
        }
        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.UserName, ClaimValueTypes.String));
        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        claims.Add(new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName ?? "Unspecified"));
        claims.Add(new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName ?? "Unspecified"));
        claims.Add(new Claim(ClaimTypes.HomePhone, phone, ClaimValueTypes.String));
        claims.Add(new Claim(ClaimTypes.Role, role ?? "Unspecified", ClaimValueTypes.String));

        if (sessionId != Guid.Empty && sessionId != null && sessionId.HasValue)
        {
            var str = sessionId.ToString();
            if (!string.IsNullOrEmpty(str))
            {
                claims.Add(new Claim("SessionId", str , ClaimValueTypes.String));
            }
        }

        return claims;
    }

    public static UserVerificationResult ValidateRefreshToken(IConfiguration configuration, string token)
    {
        var secret = configuration.GetValue<string>("Secret:Jwt:UserRefreshTokenSecret");
        if (string.IsNullOrEmpty(secret))
        {
            throw new ArgumentNullException("UserRefreshTokenSecret is missing in appsettings.json");
        }

        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secret);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var claimsPrincipal = handler.ValidateToken(token, validationParameters, out var validatedToken);
        var userIdStr = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdStr))
        {
            throw new Exception("Invalid token.");
        }
        var userId = Guid.Parse(userIdStr);
        var userName = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return new UserVerificationResult
        {
            UserId = userId,
            UserName = userName ?? "Unspecified",
            Role = claimsPrincipal.FindFirst(ClaimTypes.Role)?.Value ?? "Unspecified",
        };
    }

    public static string GenerateOtp()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    public static string GenerateResetToken(IConfiguration configuration, Guid userId, string? email)
    {
        var TOKEN_EXPIRARY_HOURS = 3;
        var secret = configuration["Password:ResetTokenSecret"];
        if (string.IsNullOrEmpty(secret))
        {
            throw new Exception("Reset token secret not found in configuration.");
        }
        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Email, email ?? "Unspecified"),
                new Claim(ClaimTypes.Sid, userId.ToString()), // Add real user ID here
            }),
            Expires = DateTime.UtcNow.AddHours(TOKEN_EXPIRARY_HOURS),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = handler.CreateToken(tokenDescriptor);
        var resetToken = handler.WriteToken(token);
        return resetToken;
    }

    public static (string email, Guid userId) ValidateResetToken(IConfiguration configuration, string token)
    {
        var secret = configuration["Password:ResetTokenSecret"];
        if (string.IsNullOrEmpty(secret))
        {
            throw new Exception("Reset token secret not found in configuration.");
        }
        var handler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(secret);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var claimsPrincipal = handler.ValidateToken(token, validationParameters, out var validatedToken);
        var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;
        var userIdStr = claimsPrincipal.FindFirst(ClaimTypes.Sid)?.Value;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(userIdStr))
        {
            throw new Exception("Invalid reset token.");
        }
        var userId = Guid.Parse(userIdStr);
        return (email, userId);
    }

    public static UserVerificationResult GetCurrentUser(HttpContext context)
    {
        // Get the ClaimsPrincipal representing the current user
        var result = new UserVerificationResult();
        var user = context.User;
        if (user == null)
        {
            return result;
        }
        if (user.Identity != null && user.Identity.IsAuthenticated)
        {
            var userIdStr = user.FindFirst(ClaimTypes.Sid)?.Value;
            if (userIdStr == null)
            {
                return result;
            }
            var sessionIdStr = user.FindFirst("SessionId")?.Value;
            if (sessionIdStr == null)
            {
                result.UserId = Guid.Parse(userIdStr);
                return result;
            }
            var tenantIdStr = user.FindFirst("TenantId")?.Value;
            if (tenantIdStr != null)
            {
                result.TenantId = Guid.Parse(tenantIdStr);
            }
            var roleName = user.FindFirst(ClaimTypes.Role)?.Value;
            result.Role = roleName ?? "Unspecified";

            var userName = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            result.UserName = userName ?? "Unspecified";

            result.UserId = Guid.Parse(userIdStr);
            result.SessionId = Guid.Parse(sessionIdStr);
            return result;
        }
        return result;
    }

}
