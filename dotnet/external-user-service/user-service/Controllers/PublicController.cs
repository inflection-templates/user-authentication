using Microsoft.AspNetCore.Mvc;

namespace AspNetJwtAuth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublicController : ControllerBase
    {
        private readonly ILogger<PublicController> _logger;

        public PublicController(ILogger<PublicController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Public endpoint - no authentication required
        /// </summary>
        [HttpGet]
        public IActionResult GetPublicInfo()
        {
            var response = new
            {
                Message = "This is a public endpoint - no authentication required",
                Service = "ASP.NET Core JWT Authentication API",
                Version = "1.0.0",
                Features = new[]
                {
                    "JWT Authentication with JWKS",
                    "Role-Based Access Control",
                    "Permission-Based Authorization",
                    "Token Blacklisting",
                    "Comprehensive Logging"
                },
                Endpoints = new
                {
                    Public = new[]
                    {
                        "GET /api/public",
                        "GET /health"
                    },
                    Authentication = new[]
                    {
                        "POST /api/auth/login",
                        "POST /api/auth/register",
                        "POST /api/auth/logout",
                        "GET /api/auth/profile",
                        "PUT /api/auth/profile",
                        "POST /api/auth/change-password"
                    },
                    Secure = new[]
                    {
                        "GET /api/secure/profile",
                        "GET /api/secure/admin",
                        "GET /api/secure/moderator",
                        "GET /api/secure/write-posts",
                        "DELETE /api/secure/posts/{id}",
                        "GET /api/secure/cache-stats",
                        "POST /api/secure/refresh-keys",
                        "GET /api/secure/users",
                        "POST /api/secure/users/{userId}/roles/{roleId}"
                    }
                },
                Documentation = new
                {
                    Swagger = "/swagger",
                    OpenAPI = "/swagger/v1/swagger.json"
                },
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Public endpoint accessed");
            return Ok(response);
        }

        /// <summary>
        /// Get API information and statistics
        /// </summary>
        [HttpGet("info")]
        public IActionResult GetApiInfo()
        {
            var response = new
            {
                API = new
                {
                    Name = "ASP.NET Core JWT Authentication API",
                    Version = "1.0.0",
                    Description = "Production-ready JWT authentication with JWKS, RBAC, and token blacklisting",
                    Framework = ".NET 6.0",
                    Author = "Your Company",
                    License = "MIT"
                },
                Security = new
                {
                    AuthenticationMethod = "JWT Bearer Token",
                    KeyManagement = "JWKS (JSON Web Key Set)",
                    TokenValidation = new[]
                    {
                        "Issuer Validation",
                        "Audience Validation", 
                        "Lifetime Validation",
                        "Signature Validation",
                        "Blacklist Check"
                    },
                    Authorization = new[]
                    {
                        "Role-Based Access Control (RBAC)",
                        "Permission-Based Authorization",
                        "Policy-Based Authorization"
                    }
                },
                Features = new
                {
                    Caching = new[]
                    {
                        "JWKS Key Caching (1 hour default)",
                        "Token Blacklist Caching (10 minutes default)",
                        "Configurable TTL values"
                    },
                    Monitoring = new[]
                    {
                        "Structured Logging with Serilog",
                        "Health Check Endpoint",
                        "Cache Statistics",
                        "Performance Metrics"
                    },
                    Database = new[]
                    {
                        "Entity Framework Core",
                        "SQLite (configurable)",
                        "Code-First Migrations",
                        "Seed Data"
                    }
                },
                Timestamp = DateTime.UtcNow
            };

            return Ok(response);
        }

        /// <summary>
        /// Get supported roles and permissions
        /// </summary>
        [HttpGet("roles-permissions")]
        public IActionResult GetRolesAndPermissions()
        {
            var response = new
            {
                Message = "Available roles and permissions in the system",
                Roles = new[]
                {
                    new { Name = "admin", Description = "Administrator role with full access" },
                    new { Name = "moderator", Description = "Moderator role with limited admin access" },
                    new { Name = "user", Description = "Standard user role" }
                },
                Permissions = new[]
                {
                    new { Name = "read:posts", Description = "Read posts", Category = "posts" },
                    new { Name = "write:posts", Description = "Create and edit posts", Category = "posts" },
                    new { Name = "delete:posts", Description = "Delete posts", Category = "posts" },
                    new { Name = "manage:users", Description = "Manage user accounts", Category = "users" },
                    new { Name = "view:analytics", Description = "View analytics and reports", Category = "analytics" }
                },
                RolePermissionMappings = new
                {
                    admin = new[] { "read:posts", "write:posts", "delete:posts", "manage:users", "view:analytics" },
                    moderator = new[] { "read:posts", "write:posts", "delete:posts" },
                    user = new[] { "read:posts", "write:posts" }
                },
                Timestamp = DateTime.UtcNow
            };

            return Ok(response);
        }

        /// <summary>
        /// Get example JWT token structure
        /// </summary>
        [HttpGet("token-example")]
        public IActionResult GetTokenExample()
        {
            var response = new
            {
                Message = "Example JWT token structure expected by this API",
                TokenStructure = new
                {
                    Header = new
                    {
                        alg = "RS256",
                        typ = "JWT",
                        kid = "key-id-from-jwks"
                    },
                    Payload = new
                    {
                        sub = "user-id",
                        iss = "https://your-auth-service.com",
                        aud = "your-api-service",
                        exp = "expiration-timestamp",
                        iat = "issued-at-timestamp",
                        jti = "unique-token-id",
                        name = "username",
                        role = new[] { "user", "admin" },
                        permissions = new[] { "read:posts", "write:posts" }
                    }
                },
                RequiredClaims = new[]
                {
                    "sub (Subject/User ID)",
                    "iss (Issuer)",
                    "aud (Audience)", 
                    "exp (Expiration)",
                    "jti (JWT ID for blacklisting)"
                },
                OptionalClaims = new[]
                {
                    "name (Username)",
                    "role (User roles)",
                    "permissions (User permissions)"
                },
                ValidationRules = new[]
                {
                    "Token must be signed with RS256 algorithm",
                    "Issuer must match configured value",
                    "Audience must match configured value",
                    "Token must not be expired",
                    "Token must not be blacklisted",
                    "Signature must be valid using JWKS public key"
                },
                Timestamp = DateTime.UtcNow
            };

            return Ok(response);
        }
    }
}
