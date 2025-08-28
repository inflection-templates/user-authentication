using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Text.Json;

namespace AspNetJwtAuth.Demo
{
    /// <summary>
    /// Demo authentication service that provides JWKS endpoint and token generation
    /// This simulates an external authentication service like Auth0, Okta, etc.
    /// </summary>
    public class DemoAuthService
    {
        private readonly DemoJwtGenerator _jwtGenerator;
        private readonly ConcurrentDictionary<string, DateTime> _blacklistedTokens;
        private readonly ILogger<DemoAuthService> _logger;

        public DemoAuthService(ILogger<DemoAuthService> logger)
        {
            _jwtGenerator = new DemoJwtGenerator();
            _blacklistedTokens = new ConcurrentDictionary<string, DateTime>();
            _logger = logger;
        }

        /// <summary>
        /// Get JWKS (JSON Web Key Set) for token validation
        /// </summary>
        public string GetJwks()
        {
            return _jwtGenerator.GenerateJwks();
        }

        /// <summary>
        /// Generate a token for a user
        /// </summary>
        public string GenerateTokenForUser(string userId)
        {
            // Predefined users with different roles and permissions
            var userProfiles = new Dictionary<string, (string username, string[] roles, string[] permissions)>
            {
                ["1"] = ("admin", new[] { "admin" }, new[] { "read:posts", "write:posts", "delete:posts", "manage:users", "view:analytics" }),
                ["2"] = ("moderator", new[] { "moderator" }, new[] { "read:posts", "write:posts", "delete:posts" }),
                ["3"] = ("user123", new[] { "user" }, new[] { "read:posts", "write:posts" }),
                ["admin456"] = ("admin", new[] { "admin" }, new[] { "read:posts", "write:posts", "delete:posts", "manage:users", "view:analytics" }),
                ["user789"] = ("regularuser", new[] { "user" }, new[] { "read:posts", "write:posts" })
            };

            if (userProfiles.TryGetValue(userId, out var profile))
            {
                return _jwtGenerator.GenerateToken(userId, profile.username, profile.roles, profile.permissions);
            }

            // Default user profile
            return _jwtGenerator.GenerateToken(userId, $"user_{userId}", new[] { "user" }, new[] { "read:posts" });
        }

        /// <summary>
        /// Check if a token is blacklisted
        /// </summary>
        public bool IsTokenBlacklisted(string jti)
        {
            // Clean up expired entries
            CleanupExpiredTokens();
            
            return _blacklistedTokens.ContainsKey(jti);
        }

        /// <summary>
        /// Blacklist a token
        /// </summary>
        public void BlacklistToken(string jti, DateTime expiresAt)
        {
            _blacklistedTokens[jti] = expiresAt;
            _logger.LogInformation("Token {Jti} blacklisted until {ExpiresAt}", jti, expiresAt);
        }

        /// <summary>
        /// Get all sample tokens for testing
        /// </summary>
        public Dictionary<string, string> GetAllSampleTokens()
        {
            return DemoJwtGenerator.GetSampleTokens(_jwtGenerator);
        }

        private void CleanupExpiredTokens()
        {
            var now = DateTime.UtcNow;
            var expiredTokens = _blacklistedTokens
                .Where(kvp => kvp.Value < now)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var jti in expiredTokens)
            {
                _blacklistedTokens.TryRemove(jti, out _);
            }
        }
    }

    /// <summary>
    /// Demo controller that provides authentication endpoints
    /// </summary>
    [ApiController]
    [Route("")]
    public class DemoAuthController : ControllerBase
    {
        private readonly DemoAuthService _authService;
        private readonly ILogger<DemoAuthController> _logger;

        public DemoAuthController(DemoAuthService authService, ILogger<DemoAuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// JWKS endpoint for public key retrieval
        /// </summary>
        [HttpGet(".well-known/jwks.json")]
        public IActionResult GetJwks()
        {
            try
            {
                var jwks = _authService.GetJwks();
                _logger.LogInformation("JWKS endpoint accessed");
                return Content(jwks, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serving JWKS");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Generate JWT token for a user
        /// </summary>
        [HttpPost("auth/token")]
        public IActionResult GenerateToken([FromBody] TokenRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.UserId))
                {
                    return BadRequest(new { error = "userId is required" });
                }

                var token = _authService.GenerateTokenForUser(request.UserId);
                
                var response = new
                {
                    access_token = token,
                    token_type = "Bearer",
                    expires_in = 3600, // 1 hour
                    scope = "read write",
                    userId = request.UserId
                };

                _logger.LogInformation("Token generated for user: {UserId}", request.UserId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating token for user: {UserId}", request.UserId);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Check if token is blacklisted
        /// </summary>
        [HttpGet("api/auth/blacklist/{jti}")]
        public IActionResult CheckBlacklist(string jti)
        {
            try
            {
                var isBlacklisted = _authService.IsTokenBlacklisted(jti);
                
                if (isBlacklisted)
                {
                    _logger.LogInformation("Token {Jti} is blacklisted", jti);
                    return Ok(new { blacklisted = true });
                }
                else
                {
                    _logger.LogDebug("Token {Jti} is not blacklisted", jti);
                    return NotFound(new { blacklisted = false });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking blacklist for token: {Jti}", jti);
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Blacklist a token
        /// </summary>
        [HttpPost("api/auth/blacklist")]
        public IActionResult BlacklistToken([FromBody] BlacklistRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Jti))
                {
                    return BadRequest(new { error = "jti is required" });
                }

                var expiresAt = request.ExpiresAt ?? DateTime.UtcNow.AddHours(24);
                _authService.BlacklistToken(request.Jti, expiresAt);

                return Ok(new { message = "Token blacklisted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blacklisting token: {Jti}", request.Jti);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all sample tokens for testing
        /// </summary>
        [HttpGet("auth/samples")]
        public IActionResult GetSampleTokens()
        {
            try
            {
                var tokens = _authService.GetAllSampleTokens();
                
                var response = new
                {
                    message = "Sample JWT tokens for testing",
                    tokens = tokens,
                    usage = "Use these tokens in the Authorization header: 'Bearer <token>'",
                    expires_in = 3600
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating sample tokens");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Demo service health check
        /// </summary>
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "healthy",
                service = "Demo Auth Service",
                timestamp = DateTime.UtcNow,
                endpoints = new
                {
                    jwks = "/.well-known/jwks.json",
                    token = "/auth/token",
                    samples = "/auth/samples",
                    blacklist_check = "/api/auth/blacklist/{jti}",
                    blacklist_add = "/api/auth/blacklist"
                }
            });
        }
    }

    // Request DTOs
    public class TokenRequest
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class BlacklistRequest
    {
        public string Jti { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
    }
}
