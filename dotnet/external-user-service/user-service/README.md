# ASP.NET Core JWT Authentication - Working Example

This is a complete, production-ready implementation of JWT authentication in ASP.NET Core with support for JWKS (JSON Web Key Set), token blacklisting, and role-based access control.

## 🚀 Features

- **JWT Authentication**: Secure token validation using JWKS endpoints
- **Token Blacklisting**: Token revocation support with fail-open strategy
- **Role-Based Access Control**: Flexible authorization with roles and permissions
- **Entity Framework Core**: SQLite database with code-first migrations
- **Comprehensive Logging**: Structured logging with Serilog
- **Demo Auth Service**: Built-in JWT generator for testing
- **Swagger Documentation**: Interactive API documentation
- **Production Ready**: Error handling, security headers, and monitoring

## 📁 Project Structure

```
aspnet_jwt_auth_sample/
├── Controllers/
│   ├── AuthController.cs           # Authentication endpoints
│   ├── SecureController.cs         # Protected API endpoints
│   └── PublicController.cs         # Public information endpoints
├── Data/
│   └── ApplicationDbContext.cs     # Entity Framework context
├── Demo/
│   ├── DemoJwtGenerator.cs         # JWT token generator
│   ├── DemoAuthService.cs          # Mock auth service
│   └── DemoProgram.cs              # Demo service runner
├── Models/
│   ├── User.cs                     # User entity
│   ├── Role.cs                     # Role entity
│   ├── UserRole.cs                 # User-Role mapping
│   ├── Permission.cs               # Permission entity
│   ├── RolePermission.cs           # Role-Permission mapping
│   └── JwksModels.cs               # JWKS response models
├── Services/
│   ├── Interfaces/                 # Service interfaces
│   ├── JwksService.cs              # JWKS key management
│   ├── TokenBlacklistService.cs    # Token revocation
│   └── UserService.cs              # User management
├── Program.cs                      # Application entry point
├── appsettings.json                # Configuration
└── AspNetJwtAuth.csproj            # Project file
```

## 🛠️ Quick Start

### Prerequisites

- .NET 6.0 SDK or later
- Visual Studio 2022 or VS Code
- Git

### 1. Clone and Setup

```bash
cd aspnet_jwt_auth_sample
dotnet restore
```

### 2. Configure Environment

```bash
# Copy environment template
cp env.example .env
```

### 3. Run Demo Auth Service

Open a new terminal and run the demo authentication service:

```bash
# Run demo auth service (provides JWKS and token generation)
dotnet run --project Demo/DemoProgram.cs
```

This starts the demo service on:
- HTTP: http://localhost:5001
- HTTPS: https://localhost:5002

### 4. Run Main Application

In another terminal:

```bash
# Run main JWT authentication API
dotnet run
```

This starts the main service on:
- HTTPS: https://localhost:5000
- HTTP: http://localhost:5001

### 5. Test the System

#### Get Sample Tokens
```bash
# Get all sample tokens
curl http://localhost:5001/auth/samples

# Generate token for specific user
curl -X POST http://localhost:5001/auth/token \
     -H "Content-Type: application/json" \
     -d '{"userId": "admin456"}'
```

#### Test Protected Endpoints
```bash
# Copy token from previous response
TOKEN="your.jwt.token.here"

# Test profile endpoint
curl -H "Authorization: Bearer $TOKEN" \
     https://localhost:5000/api/secure/profile

# Test admin endpoint (requires admin role)
curl -H "Authorization: Bearer $TOKEN" \
     https://localhost:5000/api/secure/admin
```

## 🔐 API Endpoints

### Public Endpoints
- `GET /health` - Health check
- `GET /api/public` - Public information
- `GET /api/public/info` - API information
- `GET /api/public/roles-permissions` - Available roles and permissions
- `GET /api/public/token-example` - JWT token structure example

### Authentication Endpoints
- `POST /api/auth/login` - User login (returns user info)
- `POST /api/auth/register` - User registration
- `POST /api/auth/logout` - User logout (blacklists token)
- `GET /api/auth/profile` - Get user profile
- `PUT /api/auth/profile` - Update user profile
- `POST /api/auth/change-password` - Change password

### Secure Endpoints (Require Authentication)
- `GET /api/secure/profile` - User profile (any authenticated user)
- `GET /api/secure/admin` - Admin only data (requires 'admin' role)
- `GET /api/secure/moderator` - Moderator data (requires 'admin' or 'moderator' role)
- `GET /api/secure/write-posts` - Write posts (requires 'write:posts' permission)
- `DELETE /api/secure/posts/{id}` - Delete posts (requires permissions and roles)
- `GET /api/secure/cache-stats` - Cache statistics (admin only)
- `POST /api/secure/refresh-keys` - Refresh JWKS keys (admin only)
- `GET /api/secure/users` - Get all users (admin only)
- `POST /api/secure/users/{userId}/roles/{roleId}` - Assign role (admin only)

### Demo Auth Service Endpoints
- `GET /.well-known/jwks.json` - Public keys for token validation
- `POST /auth/token` - Generate JWT tokens
- `GET /auth/samples` - Get all sample tokens
- `GET /api/auth/blacklist/{jti}` - Check token blacklist
- `POST /api/auth/blacklist` - Blacklist token

## 🔑 Default Users

The application comes with pre-seeded users:

| Username | Password | Role | Permissions |
|----------|----------|------|-------------|
| admin | password123 | admin | All permissions |
| moderator | password123 | moderator | read:posts, write:posts, delete:posts |
| user123 | password123 | user | read:posts, write:posts |

## 🧪 Testing

### Using Swagger UI

1. Navigate to https://localhost:5000/swagger
2. Get a token from the demo service: `GET http://localhost:5001/auth/samples`
3. Click "Authorize" in Swagger UI
4. Enter: `Bearer {your-token}`
5. Test the endpoints

### Using cURL

```bash
# Get admin token
ADMIN_TOKEN=$(curl -s http://localhost:5001/auth/samples | jq -r '.tokens.admin')

# Test admin endpoint
curl -H "Authorization: Bearer $ADMIN_TOKEN" \
     https://localhost:5000/api/secure/admin

# Test cache stats
curl -H "Authorization: Bearer $ADMIN_TOKEN" \
     https://localhost:5000/api/secure/cache-stats
```

### Using Postman

1. Import the provided Postman collection (if available)
2. Set the `baseUrl` variable to `https://localhost:5000`
3. Set the `authServiceUrl` variable to `http://localhost:5001`
4. Run the "Get Sample Tokens" request
5. Copy tokens to the authorization settings

## ⚙️ Configuration

### Database Configuration

The application uses SQLite by default. To use a different database:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=JwtAuthDb;Trusted_Connection=true;"
  }
}
```

### JWT Configuration

```json
{
  "Jwt": {
    "Issuer": "https://your-auth-service.com",
    "Audience": "your-api-service",
    "JwksUrl": "https://your-auth-service.com/.well-known/jwks.json",
    "KeysCacheTtlMinutes": 60
  }
}
```

### Environment Variables

```bash
# Override appsettings.json values
Jwt__Issuer=https://production-auth.company.com
Jwt__Audience=production-api.company.com
Jwt__JwksUrl=https://production-auth.company.com/.well-known/jwks.json
ConnectionStrings__DefaultConnection="Server=prod-db;Database=JwtAuth;..."
```

## 🚀 Production Deployment

### 1. Build for Production

```bash
dotnet publish -c Release -o ./publish
```

### 2. Environment Configuration

```bash
export ASPNETCORE_ENVIRONMENT=Production
export Jwt__Issuer=https://auth.yourcompany.com
export Jwt__Audience=api.yourcompany.com
export Jwt__JwksUrl=https://auth.yourcompany.com/.well-known/jwks.json
```

### 3. Run Production Build

```bash
cd publish
dotnet AspNetJwtAuth.dll
```

### 4. Docker Deployment

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY publish/ .
EXPOSE 80
EXPOSE 443
ENTRYPOINT ["dotnet", "AspNetJwtAuth.dll"]
```

## 🔧 Advanced Configuration

### Custom Authorization Policies

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PremiumUser", policy =>
        policy.RequireClaim("subscription", "premium"));
    
    options.AddPolicy("CanDeletePosts", policy =>
        policy.RequireRole("admin", "moderator")
              .RequireClaim("permissions", "delete:posts"));
});
```

### Custom Middleware

```csharp
// Add custom middleware for additional token validation
app.UseMiddleware<CustomTokenValidationMiddleware>();
```

### Redis Caching

For distributed environments, replace in-memory caching with Redis:

```csharp
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
});
```

## 🔍 Monitoring and Logging

### Logs Location

- Console output during development
- File logs: `logs/jwt-auth-{date}.txt`
- Structured JSON logs in production

### Key Metrics to Monitor

- Authentication success/failure rates
- Token validation latency
- JWKS endpoint response times
- Cache hit/miss ratios
- Database query performance

### Health Checks

- `GET /health` - Basic health status
- Monitor database connectivity
- Monitor external service dependencies

## 🚨 Security Considerations

### Production Checklist

- [ ] Use HTTPS in production
- [ ] Configure proper CORS policies
- [ ] Set up rate limiting
- [ ] Monitor for suspicious activity
- [ ] Regular security audits
- [ ] Keep dependencies updated
- [ ] Use environment variables for secrets
- [ ] Implement proper logging without sensitive data

### Token Security

- Tokens expire after 1 hour by default
- Blacklisted tokens are checked on each request
- Use secure algorithms (RS256)
- Validate all JWT claims
- Implement proper key rotation

## 🔗 Integration Examples

### With React Frontend

```javascript
// Set up axios interceptor
axios.defaults.baseURL = 'https://localhost:5000';
axios.interceptors.request.use(config => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
```

### With External Auth Providers

Replace the demo service with real providers:

- Auth0: Use Auth0's JWKS endpoint
- Azure AD: Configure Azure AD endpoints
- Okta: Use Okta's JWKS endpoint
- Custom: Implement your own JWT issuer

## 📚 Additional Resources

- [ASP.NET Core Security Documentation](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [JWT.io](https://jwt.io/) - JWT debugger
- [JWKS RFC](https://tools.ietf.org/html/rfc7517)
- [OAuth 2.0 RFC](https://tools.ietf.org/html/rfc6749)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass
6. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

**Note**: This is a complete working example. For production use, ensure proper security measures, monitoring, and deployment practices are followed.
