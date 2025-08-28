# 🚀 Quick Start Guide

Get the ASP.NET Core JWT Authentication system up and running in 5 minutes!

## 📋 Prerequisites

- .NET 6.0 SDK or later
- Your favorite code editor (Visual Studio, VS Code, etc.)

## ⚡ Quick Setup

### 1. Clone and Restore

```bash
cd aspnet_jwt_auth_sample
dotnet restore
```

### 2. Start Demo Auth Service (Terminal 1)

```bash
# Run the demo authentication service
dotnet run --project Demo/DemoProgram.cs
```

This starts the demo service that provides:
- JWKS endpoint: http://localhost:5001/.well-known/jwks.json
- Token generation: http://localhost:5001/auth/token
- Sample tokens: http://localhost:5001/auth/samples

### 3. Start Main API Service (Terminal 2)

```bash
# Run the main JWT authentication API
dotnet run
```

This starts the main service on:
- HTTPS: https://localhost:5000
- HTTP: http://localhost:5001

## 🧪 Test the System

### Get a Demo Token

```bash
# Get all sample tokens
curl http://localhost:5001/auth/samples

# Or generate token for specific user
curl -X POST http://localhost:5001/auth/token \
     -H "Content-Type: application/json" \
     -d '{"userId": "admin456"}'
```

### Use the Token

```bash
# Copy the admin token from the response above
TOKEN="eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCIsImtpZCI6IjEyMzQ1..."

# Test profile endpoint
curl -H "Authorization: Bearer $TOKEN" \
     https://localhost:5000/api/secure/profile

# Test admin endpoint (requires admin role)
curl -H "Authorization: Bearer $TOKEN" \
     https://localhost:5000/api/secure/admin

# Test public endpoint (no token needed)
curl https://localhost:5000/api/public
```

## 🔍 What's Running

### Main API Service (Port 5000)
- **Health Check**: https://localhost:5000/health
- **Swagger UI**: https://localhost:5000/swagger
- **Public API**: https://localhost:5000/api/public
- **Secure API**: https://localhost:5000/api/secure/*
- **Auth API**: https://localhost:5000/api/auth/*

### Demo Auth Service (Port 5001)
- **JWKS Endpoint**: http://localhost:5001/.well-known/jwks.json
- **Token Generator**: http://localhost:5001/auth/token
- **Sample Tokens**: http://localhost:5001/auth/samples
- **Health Check**: http://localhost:5001/health

## 📚 Available Endpoints

### Public Endpoints (No Authentication)
```bash
GET  /health                           # Health check
GET  /api/public                       # Public information
GET  /api/public/info                  # API details
GET  /api/public/roles-permissions     # Available roles
GET  /api/public/token-example         # JWT structure example
```

### Authentication Endpoints
```bash
POST /api/auth/login                   # User login
POST /api/auth/register                # User registration  
POST /api/auth/logout                  # User logout
GET  /api/auth/profile                 # Get user profile
PUT  /api/auth/profile                 # Update profile
POST /api/auth/change-password         # Change password
```

### Secure Endpoints (Authentication Required)
```bash
GET  /api/secure/profile               # User profile (any user)
GET  /api/secure/admin                 # Admin only
GET  /api/secure/moderator             # Admin or moderator
GET  /api/secure/write-posts           # Requires write:posts permission
DELETE /api/secure/posts/{id}          # Requires delete:posts + admin/moderator
GET  /api/secure/cache-stats           # Cache statistics (admin only)
POST /api/secure/refresh-keys          # Refresh JWKS keys (admin only)
GET  /api/secure/users                 # Get all users (admin only)
```

## 🔑 Default Test Users

Use these users with the demo authentication:

| User ID | Username | Role | Permissions |
|---------|----------|------|-------------|
| 1 | admin | admin | All permissions |
| 2 | moderator | moderator | read:posts, write:posts, delete:posts |
| 3 | user123 | user | read:posts, write:posts |
| admin456 | admin | admin | All permissions |
| user789 | regularuser | user | read:posts, write:posts |

## 🌐 Using Swagger UI

1. Open https://localhost:5000/swagger
2. Get a token: `curl http://localhost:5001/auth/samples`
3. Copy the admin token from the response
4. Click "Authorize" button in Swagger UI
5. Enter: `Bearer {your-token}`
6. Test the endpoints interactively

## 🧪 Quick Test Script

Save this as `test.sh` (Linux/Mac) or `test.bat` (Windows):

```bash
#!/bin/bash
echo "Getting admin token..."
ADMIN_TOKEN=$(curl -s http://localhost:5001/auth/samples | jq -r '.tokens.admin')

echo "Testing public endpoint..."
curl -s https://localhost:5000/api/public | jq '.Message'

echo "Testing secure profile..."
curl -s -H "Authorization: Bearer $ADMIN_TOKEN" \
     https://localhost:5000/api/secure/profile | jq '.Message'

echo "Testing admin endpoint..."
curl -s -H "Authorization: Bearer $ADMIN_TOKEN" \
     https://localhost:5000/api/secure/admin | jq '.Message'

echo "Done!"
```

## 🚨 Troubleshooting

### Common Issues

1. **Port already in use**
   ```bash
   # Change ports in appsettings.json or use different ports
   dotnet run --urls="https://localhost:5003;http://localhost:5004"
   ```

2. **Demo service not starting**
   ```bash
   # Check if port 5001 is available
   netstat -ano | findstr :5001  # Windows
   lsof -i :5001                 # Linux/Mac
   ```

3. **HTTPS certificate issues**
   ```bash
   # Trust development certificate
   dotnet dev-certs https --trust
   ```

4. **Database issues**
   ```bash
   # Delete database and restart (it will recreate)
   rm jwt_auth*.db
   dotnet run
   ```

### Verify Setup

1. **Check demo service**: http://localhost:5001/.well-known/jwks.json
2. **Check main service**: https://localhost:5000/health
3. **Get tokens**: http://localhost:5001/auth/samples
4. **Test with token**: Use token with secure endpoints

## 🔧 Next Steps

1. **Explore the Code**: Check out the Controllers, Services, and Models
2. **Test Different Roles**: Try user vs admin vs moderator tokens
3. **Check Logs**: Look at console output and log files in `logs/`
4. **Try Registration**: Use `/api/auth/register` to create new users
5. **Monitor Cache**: Check `/api/secure/cache-stats` (admin only)
6. **Read Documentation**: See the full README.md for detailed information

## 🎯 Key Concepts Demonstrated

- **JWT Validation**: Using JWKS for public key resolution
- **Role-Based Access**: Different endpoints for different roles
- **Permission Checks**: Fine-grained permissions like "write:posts"
- **Token Blacklisting**: Logout functionality that revokes tokens
- **Caching**: JWKS keys and blacklist caching for performance
- **Security**: Proper token validation and claim checking

---

**Happy Testing! 🎉**

For detailed documentation, configuration options, and production deployment guides, see the full [README.md](README.md).
