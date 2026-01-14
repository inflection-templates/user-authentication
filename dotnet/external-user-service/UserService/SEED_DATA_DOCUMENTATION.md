# .NET User Service - Data Seeding Documentation

## Overview

The .NET User Service (shala.api) implements a streamlined data seeding system that automatically initializes the database with essential data required for the application to function properly. The seeding process runs during application startup and ensures that default tenants, roles, client applications, and system administrators are properly configured.

## Architecture

### Seeding Flow

The data seeding process follows a specific order to maintain referential integrity:

1. **Extract Platform Information** - Load platform metadata from configuration
2. **Seed Default Tenant** - Create the default tenant organization
3. **Add System Roles** - Create default roles (SystemAdmin, SystemUser, User)
4. **Create System Admin** - Create the initial system administrator user
5. **Seed Default Client Apps** - Create client applications with API keys

### Core Components

#### 1. Seeder Class (`startup/Seeder.cs`)

The main orchestrator that coordinates the entire seeding process:

```csharp
public static async Task SeedAsync(
    IServiceProvider serviceProvider,
    bool isDevEnvironment = false,
    bool isTestEnvironment = false)
{
    var platformInfo = PlatformInfoHandler.GetPlatformInfo();
    using (var scope = serviceProvider.CreateScope())
    {
        var provider = scope.ServiceProvider;
        var tenant = await seedDefaultTenant(provider);
        await addSystemRoles(provider, tenant?.Id);
        await createSystemAdmin(provider, tenant?.Id);
        await seedDefaultClientApps(provider, isDevEnvironment, isTestEnvironment);
    }
}
```

#### 2. Application Integration

The seeding is integrated into the application startup pipeline through:

- **Program.cs**: Main entry point that calls configuration extensions
- **AppExtensions.cs**: Configures the application pipeline and calls `UseSeeder()`
- **AppSeederExtensions.cs**: Extension method that executes the seeding process

#### 3. Platform Information Handler

Extracts platform metadata from `platform.info.json` for application branding and configuration.

## Seed Data Structure

### Directory Layout

```
static.content/seed.data/
├── default.tenant.seed.json              # Default tenant configuration
├── default.tenant.seed.sample.json       # Sample tenant template
├── system.admin.seed.json                # System administrator user
├── system.admin.seed.sample.json         # Sample admin template
├── default.client.app.seed.json          # Default client application
├── default.client.app.seed.sample.json   # Sample client template
└── platform.info.json                   # Platform information and branding
```

## Detailed Seed Data Specifications

### 1. Tenant Seeding

**File**: `default.tenant.seed.json`

**Purpose**: Creates the default tenant organization that serves as the root organizational unit.

**File Location**: `static.content/seed.data/default.tenant.seed.json`

**Structure**: JSON object with the following properties:
- `Name`: Tenant display name
- `Description`: Brief description of the tenant
- `Code`: Unique identifier for the tenant
- `CountryCode`: Default country code for phone numbers
- `PhoneNumber`: Contact phone number
- `Email`: Contact email address
- `Password`: Default password for tenant authentication

**Note**: Add your tenant data to the `default.tenant.seed.json` file.

**Implementation**: `seedDefaultTenant()`
- Checks if default tenant exists by code
- Creates tenant with predefined structure
- Implements fallback mechanism for existing tenants
- Provides comprehensive error handling and logging

**Key Features**:
- **Idempotent Operation**: Safely handles existing tenants
- **Fallback Recovery**: Attempts to retrieve existing tenant on creation failure
- **Comprehensive Logging**: Detailed logging for troubleshooting
- **Error Resilience**: Graceful handling of creation failures

### 2. Role Seeding

**Purpose**: Establishes the role-based access control hierarchy using enum-based definitions.

**Default Roles** (from `DefaultRoles` enum):

| Role | Description | Enum Value |
|------|-------------|------------|
| `SystemAdmin` | System Administrator | 0 |
| `SystemUser` | System User | 1 |
| `User` | User | 2 |

**Role Structure**:
```csharp
public class RoleCreateModel
{
    public string Name { get; set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public Guid? TenantId { get; set; }
    public bool IsDefaultRole { get; set; } = false;
}
```

**Implementation**: `addSystemRoles()`
- Uses `EnumHelper.GetEnumWithNameAndDescriptions<DefaultRoles>()` to extract role definitions
- Creates roles with descriptions from enum attributes
- Associates roles with the default tenant
- Marks roles as default system roles

**Key Features**:
- **Enum-Driven**: Role definitions are maintained in strongly-typed enums
- **Automatic Description Extraction**: Uses reflection to extract descriptions from attributes
- **Tenant Association**: All roles are associated with the default tenant
- **Duplicate Prevention**: Checks for existing roles before creation

### 3. System Administrator Seeding

**File**: `system.admin.seed.json`

**Purpose**: Creates the initial system administrator account with full privileges.

**File Location**: `static.content/seed.data/system.admin.seed.json`

**Structure**: JSON object with the following properties:
- `FirstName`: Administrator's first name
- `LastName`: Administrator's last name
- `CountryCode`: Country code for phone number
- `PhoneNumber`: Administrator's phone number
- `Email`: Administrator's email address
- `UserName`: Login username
- `Password`: Login password

**Note**: Add your system administrator data to the `system.admin.seed.json` file.

**Implementation**: `createSystemAdmin()`
- Loads admin configuration from JSON file
- Creates user account with SystemAdmin role
- Sets up authentication profile with hashed password
- Implements tenant ID validation and fallback

**Key Features**:
- **Secure Password Handling**: Uses BCrypt for password hashing
- **Role Assignment**: Automatically assigns SystemAdmin role
- **Tenant Validation**: Ensures proper tenant association
- **Duplicate Prevention**: Checks for existing admin users
- **Authentication Profile**: Creates complete authentication setup

### 4. Client Application Seeding

**File**: `default.client.app.seed.json`

**Purpose**: Creates client applications for API access and authentication.

**File Location**: `static.content/seed.data/default.client.app.seed.json`

**Structure**: Array of client application objects with the following properties:
- `Code`: Unique identifier for the client
- `Name`: Display name for the client application
- `Description`: Brief description of the client's purpose
- `Type`: Client type (Internal/External)
- `RedirectUri`: OAuth redirect URI
- `LogoUrl`: URL to the client's logo
- `WebsiteUrl`: Client's website URL
- `PrivacyPolicyUrl`: URL to privacy policy
- `TermsOfServiceUrl`: URL to terms of service

**Note**: Add your client application data to the `default.client.app.seed.json` file.

**Implementation**: `seedDefaultClientApps()`
- Creates client applications with generated API keys
- Generates secure API key and secret pairs
- Associates clients with system admin as owner
- Handles environment-specific key distribution

**API Key Management**:
```csharp
var apiKey = Helper.GenerateKey(32);
var apiSecret = Helper.GenerateKey(32);
var apiSecretHash = BCrypt.Net.BCrypt.HashPassword(apiSecret);
```

**Environment-Specific Behavior**:

#### Development Environment
- **Logging**: API keys and secrets are logged to console
- **File Storage**: Keys are saved to JSON files in the application directory
- **Format**: `{client_name}_apikeys.json`

#### Test Environment
- **Logging**: API keys and secrets are logged to console
- **File Storage**: Keys are saved to JSON files for testing

#### Production Environment
- **Email Delivery**: API keys are sent to the owner via email
- **Security**: Keys are not logged or stored locally
- **Notification**: Email service handles secure key distribution


**Usage**:
- **Branding**: Provides platform branding information
- **Contact Information**: Support email and phone details
- **Legal Links**: Privacy policy and terms of service URLs
- **Social Media**: Links to social media profiles

## Permission System

### Permission Scopes

The system defines five permission scopes:

| Scope | Description | Usage |
|-------|-------------|-------|
| `System` | System-wide permissions | Administrative operations |
| `Tenant` | Tenant-specific permissions | Organization-level operations |
| `User` | User-specific permissions | Personal data operations |
| `Restricted` | Restricted access permissions | Limited access operations |
| `Public` | Publicly accessible permissions | Authentication and public APIs |

**Enum Definition**:
```csharp
public enum PermissionScope
{
    [Description("System")]
    System = 0,
    
    [Description("Tenant")]
    Tenant,
    
    [Description("User")]
    User,
    
    [Description("Restricted")]
    Restricted,
    
    [Description("Public")]
    Public
}
```

### Role Hierarchy

The system implements a three-tier role hierarchy:

1. **SystemAdmin**: Full system access with all permissions
2. **SystemUser**: Elevated privileges for system management
3. **User**: Basic user permissions for personal operations

## Configuration and Customization

### Environment-Based Configuration

### Sample Files

The system provides `.sample.json` files as templates:
- `default.tenant.seed.sample.json`
- `system.admin.seed.sample.json`
- `default.client.app.seed.sample.json`

### File Path Resolution

Seed data files are located using assembly-relative paths:

```csharp
private static string GetSeedDataFolderPath()
{
    var cwd = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
    var staticContentPath = Path.Combine(cwd, "static.content", "seed.data");
    return staticContentPath;
}
```

## Security Considerations

### Password Management
- **BCrypt Hashing**: All passwords are hashed using BCrypt
- **Secure Generation**: API keys and secrets use cryptographically secure generation
- **Environment Separation**: Production keys are never logged or stored locally

### API Key Security
- **32-Character Keys**: API keys and secrets are 32 characters long
- **Secure Hashing**: API secrets are hashed before storage
- **Environment-Specific Distribution**: Keys are distributed based on environment

### Access Control
- **Role-Based Security**: Users are assigned appropriate roles
- **Tenant Isolation**: All entities are associated with tenants
- **Permission Scoping**: Permissions are scoped to prevent privilege escalation

## Error Handling and Resilience

### Comprehensive Logging
The seeding process provides detailed logging using Serilog:

```csharp
Log.Information("Seeding default tenant...");
Log.Error("Tenant model is invalid or null for seeding.");
Log.Warning("Tenant ID is null or empty. Attempting to retrieve default tenant.");
```

### Fallback Mechanisms
- **Existing Entity Handling**: Gracefully handles pre-existing entities
- **Tenant Fallback**: Attempts to retrieve existing tenant on creation failure
- **Role Validation**: Ensures roles exist before user assignment

### Exception Management
- **Try-Catch Blocks**: All seeding operations are wrapped in exception handling
- **Graceful Degradation**: Failures in one component don't stop the entire process
- **Detailed Error Reporting**: Exceptions are logged with full details

## Integration with Application Startup

### Startup Pipeline

The seeding is integrated into the ASP.NET Core startup pipeline:

1. **Program.cs**: Entry point calls `builder.AddConfigs()` and `app.UseConfigs()`
2. **BuilderExtensions.cs**: Configures services and dependencies
3. **AppExtensions.cs**: Sets up middleware pipeline and calls `app.UseSeeder()`
4. **AppSeederExtensions.cs**: Executes the seeding process

### Dependency Injection

The seeder uses dependency injection to access services:

```csharp
var tenantService = serviceProvider.GetRequiredService<ITenantService>();
var roleService = serviceProvider.GetRequiredService<IRoleService>();
var userService = serviceProvider.GetRequiredService<IUserService>();
```

### Asynchronous Execution

Seeding operations are executed asynchronously:

```csharp
public static void Seed(IServiceProvider provider, bool isDevEnvironment = false, bool isTestEnvironment = false)
{
    Task.Run(async () =>
    {
        await SeedAsync(provider, isDevEnvironment, isTestEnvironment);
    });
}
```

## Monitoring and Verification

### Startup Logging

The application provides comprehensive startup logging:

```
🚀 Starting User Service...
📊 Environment: Development
🔧 Loading configurations...
⚙️ Configuring application...
🎯 User Service configuration complete!
```

### Service Readiness

Application readiness is logged with key endpoints:

```
🎉 User Service is ready!
🌐 Service URLs:
   • Main Service: http://localhost:5000
   • JWKS Endpoint: http://localhost:5000/.well-known/jwks.json
   • Swagger UI: http://localhost:5000/swagger
   • Health Check: http://localhost:5000/health
```

### Health Checks

The system includes health check endpoints:

```csharp
app.MapHealthChecks("/health-check", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

## Best Practices

### Development
- Use sample files as templates for custom configurations
- Test seeding process in development environment
- Validate JSON syntax before deployment
- Monitor logs for seeding success/failure

### Production
- Secure seed files with appropriate file permissions
- Use environment-specific configurations
- Implement proper backup strategies for seed data
- Monitor seeding process through logging

### Maintenance
- Regularly review and update role definitions
- Audit user accounts and permissions
- Update client application configurations as needed
- Rotate API keys and passwords periodically

## Troubleshooting

### Common Issues

1. **Seeding Failures**
   - Check database connectivity and permissions
   - Verify seed file JSON syntax and structure
   - Ensure proper file system permissions
   - Review application logs for detailed error messages

2. **Role Creation Errors**
   - Validate enum definitions in `DefaultRoles`
   - Check tenant existence before role creation
   - Verify service registration in dependency injection

3. **Client Application Issues**
   - Ensure unique client codes across applications
   - Validate API key generation and storage
   - Check email service configuration for production

4. **Authentication Problems**
   - Verify password hashing and storage
   - Check user-role associations
   - Validate authentication profile creation

### Verification Steps

After seeding, verify the following:
- Default tenant exists with code 'default'
- All three default roles are created (SystemAdmin, SystemUser, User)
- System admin user can authenticate successfully
- Client applications have valid API keys
- Platform information is loaded correctly

### Log Analysis

Monitor the following log patterns:
- `"Seeding default tenant..."` - Tenant creation start
- `"Default tenant seeded successfully"` - Tenant creation success
- `"Seeding system roles..."` - Role creation start
- `"System admin account seeded successfully"` - Admin creation success
- `"Client app {name} created with default API key"` - Client app success

## Comparison with Node.js Implementation

### Key Differences

| Aspect | .NET Implementation | Node.js Implementation |
|--------|-------------------|------------------------|
| **Permission Management** | Enum-based scopes only | Detailed permission definitions with modules |
| **Role-Permission Mapping** | Implicit through roles | Explicit JSON-based mapping |
| **Configuration** | Strongly-typed models | JSON-based configuration |
| **Error Handling** | Comprehensive try-catch with fallbacks | Service-level error handling |
| **Environment Handling** | Built-in environment detection | Manual environment configuration |
| **API Key Management** | Integrated generation and distribution | Manual configuration |

### Architectural Approach

The .NET implementation focuses on:
- **Simplicity**: Streamlined seeding process with essential components
- **Type Safety**: Strongly-typed models and enum-based definitions
- **Integration**: Deep integration with ASP.NET Core pipeline
- **Environment Awareness**: Built-in environment-specific behavior

This approach provides a more streamlined but less granular seeding system compared to the Node.js implementation, making it suitable for applications that need essential seeding without complex permission hierarchies.
