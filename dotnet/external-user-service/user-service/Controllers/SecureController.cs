using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AspNetJwtAuth.Services.Interfaces;

namespace AspNetJwtAuth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SecureController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwksService _jwksService;
        private readonly ITokenBlacklistService _tokenBlacklistService;
        private readonly ILogger<SecureController> _logger;

        public SecureController(
            IUserService userService,
            IJwksService jwksService,
            ITokenBlacklistService tokenBlacklistService,
            ILogger<SecureController> logger)
        {
            _userService = userService;
            _jwksService = jwksService;
            _tokenBlacklistService = tokenBlacklistService;
            _logger = logger;
        }

        /// <summary>
        /// Get user profile - accessible by any authenticated user
        /// </summary>
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var userId = User.FindFirst("sub")?.Value;
            var username = User.FindFirst("name")?.Value ?? User.FindFirst("unique_name")?.Value;
            var roles = User.FindAll("role").Select(c => c.Value);
            var permissions = User.FindAll("permissions").Select(c => c.Value);

            var response = new
            {
                UserId = userId,
                Username = username,
                Roles = roles,
                Permissions = permissions,
                Message = "Secure profile endpoint accessed successfully",
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Profile accessed by user: {UserId}", userId);
            return Ok(response);
        }

        /// <summary>
        /// Admin only endpoint - requires 'admin' role
        /// </summary>
        [HttpGet("admin")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAdminData()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                var userCount = users.Count();
                var adminUserId = User.FindFirst("sub")?.Value;

                var response = new
                {
                    Message = "Admin only data",
                    TotalUsers = userCount,
                    AccessedBy = adminUserId,
                    Timestamp = DateTime.UtcNow,
                    AdminFeatures = new[]
                    {
                        "User Management",
                        "System Configuration",
                        "Analytics Dashboard",
                        "Security Monitoring"
                    }
                };

                _logger.LogInformation("Admin endpoint accessed by user: {UserId}", adminUserId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accessing admin endpoint");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Moderator or Admin endpoint - requires 'admin' or 'moderator' role
        /// </summary>
        [HttpGet("moderator")]
        [Authorize(Policy = "ModeratorOrAdmin")]
        public IActionResult GetModeratorData()
        {
            var userId = User.FindFirst("sub")?.Value;
            var roles = User.FindAll("role").Select(c => c.Value);

            var response = new
            {
                Message = "Moderator or Admin data",
                AccessedBy = userId,
                UserRoles = roles,
                Timestamp = DateTime.UtcNow,
                ModeratorFeatures = new[]
                {
                    "Content Moderation",
                    "User Reports",
                    "Community Guidelines",
                    "Basic Analytics"
                }
            };

            _logger.LogInformation("Moderator endpoint accessed by user: {UserId}", userId);
            return Ok(response);
        }

        /// <summary>
        /// Write posts endpoint - requires 'write:posts' permission
        /// </summary>
        [HttpGet("write-posts")]
        [Authorize(Policy = "WritePostsPermission")]
        public IActionResult GetWritePostsData()
        {
            var userId = User.FindFirst("sub")?.Value;
            var permissions = User.FindAll("permissions").Select(c => c.Value);

            var response = new
            {
                Message = "Write posts permission granted",
                AccessedBy = userId,
                UserPermissions = permissions,
                Timestamp = DateTime.UtcNow,
                WriteFeatures = new[]
                {
                    "Create New Posts",
                    "Edit Own Posts",
                    "Upload Media",
                    "Schedule Publishing"
                }
            };

            _logger.LogInformation("Write posts endpoint accessed by user: {UserId}", userId);
            return Ok(response);
        }

        /// <summary>
        /// Delete posts endpoint - requires 'delete:posts' permission AND ('admin' or 'moderator' role)
        /// </summary>
        [HttpDelete("posts/{id}")]
        [Authorize(Policy = "DeletePostsPermission")]
        [Authorize(Policy = "ModeratorOrAdmin")]
        public IActionResult DeletePost(int id)
        {
            var userId = User.FindFirst("sub")?.Value;
            var roles = User.FindAll("role").Select(c => c.Value);
            var permissions = User.FindAll("permissions").Select(c => c.Value);

            // In a real application, you would delete the actual post here
            var response = new
            {
                Message = $"Post {id} deleted successfully",
                DeletedBy = userId,
                UserRoles = roles,
                UserPermissions = permissions,
                PostId = id,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Post {PostId} deleted by user: {UserId}", id, userId);
            return Ok(response);
        }

        /// <summary>
        /// Cache statistics endpoint - Admin only
        /// </summary>
        [HttpGet("cache-stats")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetCacheStats()
        {
            try
            {
                var jwksKeyCount = await _jwksService.GetCachedKeyCountAsync();
                var blacklistedTokenCount = await _tokenBlacklistService.GetBlacklistedTokenCountAsync();

                var response = new
                {
                    Message = "Cache statistics",
                    Statistics = new
                    {
                        JwksKeys = new
                        {
                            CachedKeyCount = jwksKeyCount,
                            Status = jwksKeyCount > 0 ? "Active" : "No Keys Cached"
                        },
                        TokenBlacklist = new
                        {
                            BlacklistedTokenCount = blacklistedTokenCount,
                            Status = "Active"
                        }
                    },
                    Timestamp = DateTime.UtcNow,
                    RequestedBy = User.FindFirst("sub")?.Value
                };

                _logger.LogInformation("Cache statistics accessed by admin user: {UserId}", User.FindFirst("sub")?.Value);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cache statistics");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Refresh JWKS keys manually - Admin only
        /// </summary>
        [HttpPost("refresh-keys")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> RefreshKeys()
        {
            try
            {
                await _jwksService.RefreshKeysAsync();
                var keyCount = await _jwksService.GetCachedKeyCountAsync();

                var response = new
                {
                    Message = "JWKS keys refreshed successfully",
                    KeyCount = keyCount,
                    RefreshedBy = User.FindFirst("sub")?.Value,
                    Timestamp = DateTime.UtcNow
                };

                _logger.LogInformation("JWKS keys manually refreshed by admin user: {UserId}", User.FindFirst("sub")?.Value);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing JWKS keys");
                return StatusCode(500, new { Message = "Failed to refresh keys" });
            }
        }

        /// <summary>
        /// User management endpoint - Admin only
        /// </summary>
        [HttpGet("users")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                
                var response = new
                {
                    Message = "All users retrieved successfully",
                    Users = users.Select(u => new
                    {
                        u.Id,
                        u.Username,
                        u.Email,
                        u.FirstName,
                        u.LastName,
                        u.CreatedAt,
                        u.LastLoginAt,
                        u.IsActive,
                        Roles = u.Roles.ToList()
                    }),
                    TotalCount = users.Count(),
                    RequestedBy = User.FindFirst("sub")?.Value,
                    Timestamp = DateTime.UtcNow
                };

                _logger.LogInformation("User list accessed by admin: {UserId}", User.FindFirst("sub")?.Value);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user list");
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }

        /// <summary>
        /// Assign role to user - Admin only
        /// </summary>
        [HttpPost("users/{userId}/roles/{roleId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AssignRole(int userId, int roleId)
        {
            try
            {
                var updatedUser = await _userService.AssignRoleToUserAsync(userId, roleId);
                
                var response = new
                {
                    Message = "Role assigned successfully",
                    User = new
                    {
                        updatedUser.Id,
                        updatedUser.Username,
                        Roles = updatedUser.Roles.ToList()
                    },
                    AssignedBy = User.FindFirst("sub")?.Value,
                    Timestamp = DateTime.UtcNow
                };

                _logger.LogInformation("Role {RoleId} assigned to user {UserId} by admin {AdminId}", 
                    roleId, userId, User.FindFirst("sub")?.Value);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {RoleId} to user {UserId}", roleId, userId);
                return StatusCode(500, new { Message = "Internal server error" });
            }
        }
    }
}
