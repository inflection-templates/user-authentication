using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AspNetJwtAuth.Models;
using AspNetJwtAuth.Services.Interfaces;

namespace AspNetJwtAuth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ITokenBlacklistService _tokenBlacklistService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUserService userService,
            ITokenBlacklistService tokenBlacklistService,
            IJwtTokenService jwtTokenService,
            ILogger<AuthController> logger)
        {
            _userService = userService;
            _tokenBlacklistService = tokenBlacklistService;
            _jwtTokenService = jwtTokenService;
            _logger = logger;
        }

        /// <summary>
        /// JWKS endpoint for public key retrieval by other services
        /// </summary>
        [HttpGet(".well-known/jwks.json")]
        public IActionResult GetJwks()
        {
            try
            {
                var jwks = _jwtTokenService.GenerateJwks();
                _logger.LogInformation("JWKS endpoint accessed by external service");
                return Content(jwks, "application/json");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error serving JWKS");
                return StatusCode(500);
            }
        }

        /// <summary>
        /// Authenticate user and return user information
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { Message = "Username and password are required" });
                }

                var user = await _userService.AuthenticateAsync(request.Username, request.Password);
                if (user == null)
                {
                    return Unauthorized(new { Message = "Invalid username or password" });
                }

                // Generate JWT token for the authenticated user
                var token = _jwtTokenService.GenerateToken(user);
                
                var response = new
                {
                    Message = "Authentication successful",
                    AccessToken = token,
                    TokenType = "Bearer",
                    ExpiresIn = 3600, // 1 hour
                    User = new
                    {
                        user.Id,
                        user.Username,
                        user.Email,
                        user.FirstName,
                        user.LastName,
                        Roles = user.Roles.ToList(),
                        Permissions = user.Permissions.ToList()
                    }
                };

                _logger.LogInformation("User {Username} authenticated successfully and JWT token generated", user.Username);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for username: {Username}", request.Username);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Logout user by blacklisting the token
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var jti = User.FindFirst("jti")?.Value;
                if (!string.IsNullOrEmpty(jti))
                {
                    // Blacklist token for 24 hours (or until natural expiry)
                    await _tokenBlacklistService.BlacklistTokenAsync(jti, TimeSpan.FromHours(24));
                    _logger.LogInformation("Token blacklisted for user logout: {Jti}", jti);
                }

                return Ok(new { Message = "Logged out successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Username) || 
                    string.IsNullOrEmpty(request.Email) || 
                    string.IsNullOrEmpty(request.Password))
                {
                    return BadRequest(new { Message = "Username, email, and password are required" });
                }

                var user = new User
                {
                    Username = request.Username,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName
                };

                var createdUser = await _userService.CreateUserAsync(user, request.Password);

                // Assign default user role
                await _userService.AssignRoleToUserAsync(createdUser.Id, 3); // Default "user" role

                var response = new
                {
                    Message = "User registered successfully",
                    User = new
                    {
                        createdUser.Id,
                        createdUser.Username,
                        createdUser.Email,
                        createdUser.FirstName,
                        createdUser.LastName
                    }
                };

                _logger.LogInformation("New user registered: {Username}", createdUser.Username);
                return CreatedAtAction(nameof(GetProfile), new { id = createdUser.Id }, response);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for username: {Username}", request.Username);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get current user profile
        /// </summary>
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { Message = "Invalid user token" });
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                var response = new
                {
                    user.Id,
                    user.Username,
                    user.Email,
                    user.FirstName,
                    user.LastName,
                    user.CreatedAt,
                    user.LastLoginAt,
                    Roles = user.Roles.ToList(),
                    Permissions = user.Permissions.ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { Message = "Invalid user token" });
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                // Update user fields
                if (!string.IsNullOrEmpty(request.FirstName))
                    user.FirstName = request.FirstName;
                
                if (!string.IsNullOrEmpty(request.LastName))
                    user.LastName = request.LastName;

                if (!string.IsNullOrEmpty(request.Email))
                    user.Email = request.Email;

                var updatedUser = await _userService.UpdateUserAsync(user);

                var response = new
                {
                    Message = "Profile updated successfully",
                    User = new
                    {
                        updatedUser.Id,
                        updatedUser.Username,
                        updatedUser.Email,
                        updatedUser.FirstName,
                        updatedUser.LastName
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Change user password
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { Message = "Invalid user token" });
                }

                var user = await _userService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                // Verify current password
                if (!await _userService.VerifyPasswordAsync(user, request.CurrentPassword))
                {
                    return BadRequest(new { Message = "Current password is incorrect" });
                }

                // Update password
                await _userService.UpdatePasswordAsync(user, request.NewPassword);

                return Ok(new { Message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Check if a token is blacklisted (for other services)
        /// </summary>
        [HttpGet("blacklist/{jti}")]
        public async Task<IActionResult> CheckBlacklist(string jti)
        {
            try
            {
                if (string.IsNullOrEmpty(jti))
                {
                    return BadRequest(new { Message = "JTI is required" });
                }

                bool isBlacklisted = await _tokenBlacklistService.IsTokenBlacklistedAsync(jti);
                
                if (isBlacklisted)
                {
                    return Ok(new { Message = "Token is blacklisted", Jti = jti });
                }
                else
                {
                    return NotFound(new { Message = "Token is not blacklisted", Jti = jti });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking token blacklist for JTI: {Jti}", jti);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Add a token to blacklist (for other services)
        /// </summary>
        [HttpPost("blacklist")]
        public async Task<IActionResult> BlacklistToken([FromBody] BlacklistRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Jti))
                {
                    return BadRequest(new { Message = "JTI is required" });
                }

                var expiry = request.ExpiresAt.HasValue 
                    ? request.ExpiresAt.Value - DateTime.UtcNow 
                    : TimeSpan.FromHours(24);

                if (expiry <= TimeSpan.Zero)
                {
                    expiry = TimeSpan.FromHours(1); // Default to 1 hour if already expired
                }

                await _tokenBlacklistService.BlacklistTokenAsync(request.Jti, expiry);
                
                return Ok(new { Message = "Token blacklisted successfully", Jti = request.Jti });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error blacklisting token: {Jti}", request?.Jti);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }
    }

    // Request/Response DTOs
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public class UpdateProfileRequest
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class BlacklistRequest
    {
        public string Jti { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
    }
}
