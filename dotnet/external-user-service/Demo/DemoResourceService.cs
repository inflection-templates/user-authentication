using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace AspNetJwtAuth.Demo
{
    /// <summary>
    /// Demo Resource Service that only validates JWT tokens
    /// This simulates a microservice that consumes tokens from the User Service
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requires valid JWT token
    public class DemoResourceController : ControllerBase
    {
        private readonly ILogger<DemoResourceController> _logger;

        public DemoResourceController(ILogger<DemoResourceController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Get user information from JWT token claims
        /// </summary>
        [HttpGet("user-info")]
        public IActionResult GetUserInfo()
        {
            try
            {
                var userClaims = new
                {
                    UserId = User.FindFirst("sub")?.Value,
                    Username = User.FindFirst("name")?.Value,
                    Email = User.FindFirst("email")?.Value,
                    FirstName = User.FindFirst("firstName")?.Value,
                    LastName = User.FindFirst("lastName")?.Value,
                    Roles = User.FindAll("role").Select(c => c.Value).ToList(),
                    Permissions = User.FindAll("permissions").Select(c => c.Value).ToList(),
                    TokenIssuedAt = User.FindFirst("iat")?.Value,
                    TokenExpiresAt = User.FindFirst("exp")?.Value,
                    TokenId = User.FindFirst("jti")?.Value
                };

                _logger.LogInformation("User info retrieved from token for user: {Username}", userClaims.Username);

                return Ok(new
                {
                    Message = "User information retrieved from JWT token",
                    User = userClaims,
                    Service = "Demo Resource Service",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user info from token");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get user roles from JWT token
        /// </summary>
        [HttpGet("user-roles")]
        public IActionResult GetUserRoles()
        {
            try
            {
                var roles = User.FindAll("role").Select(c => c.Value).ToList();
                
                _logger.LogInformation("User roles retrieved from token: {Roles}", string.Join(", ", roles));

                return Ok(new
                {
                    Message = "User roles retrieved from JWT token",
                    Roles = roles,
                    Service = "Demo Resource Service",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user roles from token");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Get user permissions from JWT token
        /// </summary>
        [HttpGet("user-permissions")]
        public IActionResult GetUserPermissions()
        {
            try
            {
                var permissions = User.FindAll("permissions").Select(c => c.Value).ToList();
                
                _logger.LogInformation("User permissions retrieved from token: {Permissions}", string.Join(", ", permissions));

                return Ok(new
                {
                    Message = "User permissions retrieved from JWT token",
                    Permissions = permissions,
                    Service = "Demo Resource Service",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user permissions from token");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Check if user has specific permission
        /// </summary>
        [HttpGet("check-permission/{permission}")]
        public IActionResult CheckPermission(string permission)
        {
            try
            {
                var userPermissions = User.FindAll("permissions").Select(c => c.Value).ToList();
                var hasPermission = userPermissions.Contains(permission);

                _logger.LogInformation("Permission check for '{Permission}': {HasPermission}", permission, hasPermission);

                return Ok(new
                {
                    Message = $"Permission check for '{permission}'",
                    Permission = permission,
                    HasPermission = hasPermission,
                    UserPermissions = userPermissions,
                    Service = "Demo Resource Service",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking permission: {Permission}", permission);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Check if user has specific role
        /// </summary>
        [HttpGet("check-role/{role}")]
        public IActionResult CheckRole(string role)
        {
            try
            {
                var userRoles = User.FindAll("role").Select(c => c.Value).ToList();
                var hasRole = userRoles.Contains(role);

                _logger.LogInformation("Role check for '{Role}': {HasRole}", role, hasRole);

                return Ok(new
                {
                    Message = $"Role check for '{role}'",
                    Role = role,
                    HasRole = hasRole,
                    UserRoles = userRoles,
                    Service = "Demo Resource Service",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking role: {Role}", role);
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Admin-only endpoint
        /// </summary>
        [HttpGet("admin-only")]
        [Authorize(Roles = "admin")]
        public IActionResult AdminOnly()
        {
            _logger.LogInformation("Admin endpoint accessed by user: {Username}", User.FindFirst("name")?.Value);

            return Ok(new
            {
                Message = "Admin-only endpoint accessed successfully",
                User = User.FindFirst("name")?.Value,
                Service = "Demo Resource Service",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Moderator or Admin endpoint
        /// </summary>
        [HttpGet("moderator-or-admin")]
        [Authorize(Roles = "moderator,admin")]
        public IActionResult ModeratorOrAdmin()
        {
            _logger.LogInformation("Moderator/Admin endpoint accessed by user: {Username}", User.FindFirst("name")?.Value);

            return Ok(new
            {
                Message = "Moderator or Admin endpoint accessed successfully",
                User = User.FindFirst("name")?.Value,
                Roles = User.FindAll("role").Select(c => c.Value).ToList(),
                Service = "Demo Resource Service",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Service health check
        /// </summary>
        [HttpGet("health")]
        [AllowAnonymous]
        public IActionResult Health()
        {
            return Ok(new
            {
                status = "healthy",
                service = "Demo Resource Service",
                description = "This service validates JWT tokens from User Service",
                timestamp = DateTime.UtcNow,
                endpoints = new
                {
                    user_info = "/api/demoresource/user-info",
                    user_roles = "/api/demoresource/user-roles",
                    user_permissions = "/api/demoresource/user-permissions",
                    check_permission = "/api/demoresource/check-permission/{permission}",
                    check_role = "/api/demoresource/check-role/{role}",
                    admin_only = "/api/demoresource/admin-only",
                    moderator_or_admin = "/api/demoresource/moderator-or-admin"
                }
            });
        }

        // ========================================
        // DECENTRALIZED PERMISSIONS EXAMPLES
        // ========================================
        // These endpoints demonstrate how different services can enforce
        // their own business rules based on JWT claims without calling User Service

        /// <summary>
        /// Order Service Example - Buyer/Seller specific logic
        /// This service cares about "buyer" and "seller" roles
        /// </summary>
        [HttpGet("order-service/place-order")]
        [Authorize(Roles = "buyer,seller")]
        public IActionResult PlaceOrder()
        {
            var userRole = User.FindFirst("role")?.Value;
            var username = User.FindFirst("name")?.Value;

            var response = new
            {
                Message = "Order placed successfully",
                Service = "Order Service (Example)",
                UserRole = userRole,
                Username = username,
                BusinessLogic = userRole == "buyer" 
                    ? "Buyer can place orders up to $1000" 
                    : "Seller can place orders up to $10000",
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Order placed by {Role} user: {Username}", userRole, username);
            return Ok(response);
        }

        /// <summary>
        /// Inventory Service Example - Warehouse specific logic
        /// This service cares about "warehouse_manager" role
        /// </summary>
        [HttpGet("inventory-service/update-stock")]
        [Authorize(Roles = "warehouse_manager,admin")]
        public IActionResult UpdateStock()
        {
            var userRoles = User.FindAll("role").Select(c => c.Value).ToList();
            var username = User.FindFirst("name")?.Value;

            var response = new
            {
                Message = "Stock updated successfully",
                Service = "Inventory Service (Example)",
                UserRoles = userRoles,
                Username = username,
                BusinessLogic = "Warehouse managers can update stock levels and locations",
                Permissions = User.FindAll("permissions").Select(c => c.Value).ToList(),
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Stock updated by user: {Username} with roles: {Roles}", username, string.Join(", ", userRoles));
            return Ok(response);
        }

        /// <summary>
        /// Finance Service Example - Financial specific logic
        /// This service cares about "accountant" and "finance_manager" roles
        /// </summary>
        [HttpGet("finance-service/process-payment")]
        [Authorize(Roles = "accountant,finance_manager,admin")]
        public IActionResult ProcessPayment()
        {
            var userRoles = User.FindAll("role").Select(c => c.Value).ToList();
            var username = User.FindFirst("name")?.Value;

            var response = new
            {
                Message = "Payment processed successfully",
                Service = "Finance Service (Example)",
                UserRoles = userRoles,
                Username = username,
                BusinessLogic = "Financial users can process payments and manage transactions",
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Payment processed by user: {Username} with roles: {Roles}", username, string.Join(", ", userRoles));
            return Ok(response);
        }

        /// <summary>
        /// HR Service Example - Employee specific logic
        /// This service cares about "hr_manager" and "hr_staff" roles
        /// </summary>
        [HttpGet("hr-service/view-employee")]
        [Authorize(Roles = "hr_manager,hr_staff,admin")]
        public IActionResult ViewEmployee()
        {
            var userRoles = User.FindAll("role").Select(c => c.Value).ToList();
            var username = User.FindFirst("name")?.Value;

            var response = new
            {
                Message = "Employee information retrieved",
                Service = "HR Service (Example)",
                UserRoles = userRoles,
                Username = username,
                BusinessLogic = "HR users can view and manage employee information",
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Employee viewed by user: {Username} with roles: {Roles}", username, string.Join(", ", userRoles));
            return Ok(response);
        }

        /// <summary>
        /// Demonstrate how services can implement custom business logic
        /// based on JWT claims without any User Service calls
        /// </summary>
        [HttpGet("business-logic/custom-rules")]
        [Authorize]
        public IActionResult CustomBusinessRules()
        {
            var userRoles = User.FindAll("role").Select(c => c.Value).ToList();
            var userPermissions = User.FindAll("permissions").Select(c => c.Value).ToList();
            var username = User.FindFirst("name")?.Value;

            // Custom business logic based on JWT claims
            var businessRules = new List<string>();
            
            if (userRoles.Contains("admin"))
            {
                businessRules.Add("Full system access");
                businessRules.Add("Can override any restrictions");
            }
            
            if (userRoles.Contains("buyer"))
            {
                businessRules.Add("Can place orders");
                businessRules.Add("Can view product catalog");
            }
            
            if (userRoles.Contains("warehouse_manager"))
            {
                businessRules.Add("Can manage inventory");
                businessRules.Add("Can update stock levels");
            }
            
            if (userPermissions.Contains("write:posts"))
            {
                businessRules.Add("Can create content");
            }

            var response = new
            {
                Message = "Custom business rules applied",
                Service = "Business Logic Service (Example)",
                Username = username,
                UserRoles = userRoles,
                UserPermissions = userPermissions,
                AppliedBusinessRules = businessRules,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Custom business rules applied for user: {Username}", username);
            return Ok(response);
        }
    }
}
