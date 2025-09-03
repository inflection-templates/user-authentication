# User Authentication Service - Python FastAPI

A comprehensive Python FastAPI implementation of the User Authentication Service, equivalent to the .NET shala.api UserService.

## Features

- **User Authentication**: Password-based and OTP-based login
- **User Registration**: Complete user registration flow
- **JWT Tokens**: RSA-signed JWT tokens with JWKS endpoint
- **Password Management**: Password reset, change password
- **OAuth Integration**: Support for Google, Facebook, Microsoft, GitHub (configurable)
- **Multi-factor Authentication**: OTP support via email
- **Role-based Authorization**: User roles and permissions
- **Session Management**: Login sessions with activity tracking
- **Database**: SQLite/PostgreSQL with SQLAlchemy async support
- **API Documentation**: Auto-generated OpenAPI/Swagger docs

## Architecture

```
UserService/
├── app.py                     # Main FastAPI application
├── domain/
│   └── types/
│       └── user_types.py      # Domain models and types
├── database/
│   ├── entities/
│   │   └── user_entities.py   # SQLAlchemy database entities
│   └── db_context.py         # Database configuration and seeding
├── services/
│   ├── jwt_token_service.py   # JWT token generation and validation
│   └── user_auth_service.py   # User authentication logic
├── api/
│   ├── router.py             # Main API router
│   ├── users/
│   │   └── auth/
│   │       └── routes.py     # Authentication endpoints
│   └── wellknown/
│       └── routes.py         # JWKS and discovery endpoints
├── startup/
│   ├── configurations.py     # Application configuration
│   └── middleware.py         # Middleware setup
├── appsettings.json          # Configuration file
└── requirements.txt          # Python dependencies
```

## Setup and Installation

1. **Install Dependencies**:
   ```bash
   pip install -r requirements.txt
   ```

2. **Environment Variables** (optional):
   ```bash
   export DATABASE_URL="sqlite+aiosqlite:///./user_service.db"
   export JWT_ISSUER="shala"
   export JWT_AUDIENCE="shala"
   export JWT_ACCESS_TOKEN_VALIDITY_DAYS="5"
   export JWT_REFRESH_TOKEN_VALIDITY_DAYS="365"
   ```

3. **Run the Service**:
   ```bash
   python app.py
   ```
   Or using uvicorn directly:
   ```bash
   uvicorn app:app --host 0.0.0.0 --port 5000 --reload
   ```

## Service URLs

- **API Base**: http://localhost:5000/api/v1
- **Health Check**: http://localhost:5000/health
- **API Documentation**: http://localhost:5000/docs
- **JWKS Endpoint**: http://localhost:5000/.well-known/jwks.json
- **OpenID Configuration**: http://localhost:5000/.well-known/openid_configuration

## API Endpoints

### Authentication
- `POST /api/v1/auth/login` - Login with email/password
- `POST /api/v1/auth/login/otp` - Login with email/OTP
- `POST /api/v1/auth/register` - Register new user
- `POST /api/v1/auth/send-otp` - Send OTP to email
- `POST /api/v1/auth/refresh` - Refresh access token
- `POST /api/v1/auth/logout` - Logout user
- `POST /api/v1/auth/password/reset/send-link` - Send password reset link
- `POST /api/v1/auth/password/reset` - Reset password with token
- `POST /api/v1/auth/password/change` - Change password (authenticated)

### Users
- `POST /api/v1/users` - Register user (alternative endpoint)
- `GET /api/v1/users/me` - Get current user profile
- `PUT /api/v1/users/me` - Update current user profile

### Roles
- `GET /api/v1/roles/` - Get all roles (placeholder)
- `POST /api/v1/roles/` - Create role (placeholder)

### Well-known Endpoints
- `GET /.well-known/jwks.json` - JSON Web Key Set for token validation
- `GET /.well-known/openid_configuration` - OpenID Connect configuration

## Authentication

The service uses JWT tokens with RSA-256 signing. Include the Bearer token in the Authorization header:

```
Authorization: Bearer <your-jwt-token>
```

### Default Users

The service creates default users during initialization:

1. **System Admin**:
   - Email: `admin@example.com`
   - Password: `Admin@123`
   - Role: SystemAdmin

2. **Sample User**:
   - Email: `john.doe@example.com`
   - Password: `User@123`
   - Role: User

## JWT Token Structure

Access tokens contain the following claims:
- `sub`: User ID
- `email`: User email
- `username`: Username (if set)
- `given_name`: First name
- `family_name`: Last name
- `role`: User role
- `tenant_id`: Tenant ID (if multi-tenant)
- `session_id`: Login session ID
- `timezone`: User timezone
- `is_active`: User active status
- `status`: User status

## Database

The service supports SQLite (default) and PostgreSQL. The database is automatically initialized with:

- Default tenant
- System roles (SystemAdmin, User)
- Default client application
- Sample users

## Configuration

Configuration can be provided through:
1. Environment variables
2. `appsettings.json` file
3. Command line arguments

Key configuration sections:
- **Database**: Connection settings
- **JWT**: Token settings and secrets
- **OAuth**: OAuth provider settings
- **Email**: SMTP settings for notifications
- **Cache**: Caching configuration
- **Telemetry**: Monitoring and tracing

## Security Features

- **Password Hashing**: bcrypt with salt
- **JWT Tokens**: RSA-256 signed tokens
- **Session Management**: Active session tracking
- **Account Lockout**: Failed login attempt protection
- **Password Reset**: Secure token-based reset
- **CORS**: Configurable cross-origin requests
- **Security Headers**: XSS, CSRF protection
- **Request Logging**: Comprehensive request tracking

## OAuth Integration

The service supports OAuth integration with:
- Google
- Facebook
- Microsoft
- GitHub

OAuth providers can be enabled/disabled through configuration.

## Development

For development, the service includes:
- Auto-reload on code changes
- Detailed error messages and logging
- SQL query logging (configurable)
- Request/response logging
- Swagger UI for API testing

## Production Considerations

For production deployment:

1. **Database**: Use PostgreSQL or MySQL
2. **Environment Variables**: Set all secrets via environment
3. **HTTPS**: Configure SSL/TLS
4. **CORS**: Restrict to specific origins
5. **Logging**: Configure structured logging
6. **Monitoring**: Set up health checks and metrics
7. **Secrets**: Use proper secret management
8. **Rate Limiting**: Implement API rate limiting
9. **Backup**: Configure database backups

## Integration with Order Service

This User Service provides JWT tokens that can be validated by the Order Service:

1. User authenticates with User Service
2. Receives JWT access token
3. Uses token to access Order Service endpoints
4. Order Service validates token using JWKS endpoint

The JWKS endpoint (`/.well-known/jwks.json`) provides the public key for token validation.
