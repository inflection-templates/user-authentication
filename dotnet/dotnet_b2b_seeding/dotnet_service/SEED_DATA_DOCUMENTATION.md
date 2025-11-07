# .NET B2B Seeding Service - Data Seeding Documentation

## Overview

The .NET B2B Seeding Service is a specialized database seeding application designed for Business-to-Business (B2B) scenarios. It provides comprehensive data seeding functionality with batch processing capabilities, multi-database support, and intelligent duplicate handling. The service is built with Entity Framework Core and follows a modular architecture similar to Python-based seeding implementations.

## Architecture

### Seeding Flow

The B2B seeding process follows a structured approach with two distinct modes:

#### Startup Mode (Default)
1. **Platform Information Extraction** - Load platform metadata
2. **Default Tenant Seeding** - Create the default tenant organization
3. **System Admin Creation** - Create the initial system administrator
4. **System Roles Addition** - Create default roles and assign to admin
5. **Default Client Apps Seeding** - Create client applications

#### Bulk Seeding Mode (B2B)
1. **Database Connection Verification** - Ensure database connectivity
2. **Tenant Seeding** - Create multiple tenant organizations
3. **User Seeding** - Create users with tenant associations
4. **Role Seeding** - Create roles and assign to users
5. **Client App Seeding** - Create client applications with owners

### Core Components

#### 1. DatabaseSeeder (`Database/Seeds/DatabaseSeeder.cs`)

The main orchestrator for bulk B2B seeding operations:

```csharp
public async Task<int> SeedAllAsync(Dictionary<string, int> counts)
{
    Console.WriteLine("[INFO] Using B2B mode");
    
    int totalCreated = 0;
    
    // Seed in dependency order
    totalCreated += await new TenantSeeder(_context).SeedAsync(counts["tenants"]);
    totalCreated += await new UserSeeder(_context).SeedAsync(counts["users"]);
    totalCreated += await new RoleSeeder(_context).SeedAsync(counts["roles"]);
    totalCreated += await new ClientAppSeeder(_context).SeedAsync(counts["clientApps"]);
    
    return totalCreated;
}
```

#### 2. BaseSeeder (`Database/Seeds/BaseSeeder.cs`)

Abstract base class providing common functionality for all seeders:

```csharp
public abstract class BaseSeeder
{
    protected readonly DatabaseContext Context;
    protected readonly int BatchSize;
    protected readonly bool Verbose;
    
    public abstract Task<int> SeedAsync(int count);
    protected async Task BatchInsertAsync<T>(List<T> objects) where T : class;
    protected DateTime GetRandomDate(DateTime? startDate = null, DateTime? endDate = null);
    protected T? GetRandomChoice<T>(List<T> choices);
    protected bool GetRandomBoolean(double trueProbability = 0.5);
}
```

#### 3. Startup Seeder (`Startup/Seeder.cs`)

Handles application startup seeding for essential data:

```csharp
public static async Task SeedAsync(IServiceProvider serviceProvider, bool isDevEnvironment = false, bool isTestEnvironment = false)
{
    var tenant = await SeedDefaultTenant(dbContext);
    var adminUser = await CreateSystemAdmin(dbContext, tenant?.Id);
    await AddSystemRoles(dbContext, tenant?.Id, adminUser?.Id);
    await SeedDefaultClientApps(dbContext, isDevEnvironment, isTestEnvironment);
}
```

## Database Models

### Entity Relationships

```
Tenant (1) ←→ (N) User
User (1) ←→ (N) Role
User (1) ←→ (N) ClientApp (as owner)
```

### Model Specifications

#### BaseModel (`Database/Models/BaseModel.cs`)

All entities inherit from BaseModel providing common fields:

```csharp
public abstract class BaseModel
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
```

#### User Model (`Database/Models/User.cs`)

Comprehensive user entity with B2B-specific fields:

```csharp
[Table("users")]
public class User : BaseModel
{
    [Required] public string Username { get; set; }
    [Required] public string Email { get; set; }
    [Required] public string Password { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? CountryCode { get; set; }
    public bool IsEmailVerified { get; set; } = false;
    public bool IsPhoneVerified { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    
    // B2B-specific fields
    public Guid? TenantId { get; set; }
    public string Role { get; set; } = "Normal User";
    public Guid? OrganizationId { get; set; }
    public bool IsOrganizationAdmin { get; set; } = false;
    public DateTime? InvitedAt { get; set; }
    public DateTime? JoinedAt { get; set; }
    public string? InvitedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
}
```

#### Tenant Model (`Database/Models/Tenant.cs`)

Comprehensive tenant entity for multi-tenancy support:

```csharp
[Table("tenants")]
public class Tenant : BaseModel
{
    [Required] public string Name { get; set; }
    [Required] public string Code { get; set; }
    public string? Domain { get; set; }
    public string? Description { get; set; }
    public string? Industry { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LogoUrl { get; set; }
    
    // Contact Information
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    
    // Address Information
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    
    // Status and Configuration
    public string Status { get; set; } = "active";
    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; } = false;
    public string? Settings { get; set; }        // JSON field
    public string? BillingConfig { get; set; }   // JSON field
    
    // Verification
    public DateTime? VerifiedAt { get; set; }
    public string? VerifiedBy { get; set; }
    public DateTime? DeletedAt { get; set; }
}
```

## Seeder Implementations

### 1. TenantSeeder (`Database/Seeds/TenantSeeder.cs`)

**Purpose**: Creates tenant organizations for multi-tenant B2B scenarios.

**Key Features**:
- **Duplicate Prevention**: Checks for existing tenants before seeding
- **Default Tenant Creation**: Creates a standard "default" tenant
- **Batch Processing**: Supports batch insertion for performance

**Implementation**:
```csharp
public override async Task<int> SeedAsync(int count = 1)
{
    // Check if tenants already exist
    var existingCount = await Context.Tenants.CountAsync();
    if (existingCount > 0)
    {
        Console.WriteLine($"[INFO] {existingCount} tenant(s) already exist, skipping seed");
        return 0;
    }
    
    // Create default tenant
    var tenant = new Tenant
    {
        Id = Guid.NewGuid(),
        Name = "Default",
        Code = "default",
        IsActive = true,
        Status = "active",
        IsVerified = true,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    
    await BatchInsertAsync(new List<Tenant> { tenant });
    return CreatedCount;
}
```

### 2. UserSeeder (`Database/Seeds/UserSeeder.cs`)

**Purpose**: Creates users with support for loading from seed data files.

**Key Features**:
- **Seed Data Integration**: Loads admin data from `system.admin.seed.json`
- **Password Hashing**: Uses BCrypt for secure password storage
- **Tenant Association**: Automatically assigns users to available tenants
- **Random Data Generation**: Creates realistic user data with random attributes

**Seed Data Loading**:
```csharp
private Dictionary<string, object>? LoadSeedData()
{
    var seedPath = Path.Combine(Directory.GetCurrentDirectory(), "seed.data", "system.admin.seed.json");
    
    if (File.Exists(seedPath))
    {
        var json = File.ReadAllText(seedPath);
        var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        
        return new Dictionary<string, object>
        {
            { "username", data.GetValueOrDefault("UserName", "admin") },
            { "firstName", data.GetValueOrDefault("FirstName", "System") },
            { "lastName", "Admin" },
            { "email", data.GetValueOrDefault("Email", "sys.admin@example.com") },
            { "password", data.GetValueOrDefault("Password", "ChangeMe123!") },
            { "phoneCode", data.GetValueOrDefault("PhoneCode", "+91") }
        };
    }
    
    return null;
}
```

**User Creation Logic**:
- **First User**: Uses seed data if available (typically system admin)
- **Subsequent Users**: Generated with random attributes from predefined lists
- **Tenant Assignment**: Randomly assigns users to available tenants
- **Password Security**: All passwords are hashed using BCrypt

### 3. RoleSeeder (`Database/Seeds/RoleSeeder.cs`)

**Purpose**: Creates roles and assigns them to users.

**Key Features**:
- **Predefined Roles**: Creates three standard roles: "Tenant", "Organization", "User"
- **User Assignment**: Assigns roles to existing users
- **Dependency Validation**: Ensures users exist before creating roles

**Role Options**:
```csharp
public static readonly List<string> RoleOptions = new List<string> { "Tenant", "Organization", "User" };
```

**Implementation**:
```csharp
public override async Task<int> SeedAsync(int count = 3)
{
    var users = await Context.Users.ToListAsync();
    if (users.Count == 0)
    {
        Console.WriteLine("[ERROR] No users found in database. Please seed users first.");
        return 0;
    }
    
    var roles = new List<Role>();
    
    // Create exactly one role of each type, assigned to different users
    for (int i = 0; i < RoleOptions.Count; i++)
    {
        var roleName = RoleOptions[i];
        var user = users[Math.Min(i, users.Count - 1)];
        
        var role = new Role
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            RoleName = roleName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        roles.Add(role);
    }
    
    await BatchInsertAsync(roles);
    return CreatedCount;
}
```

### 4. ClientAppSeeder (`Database/Seeds/ClientAppSeeder.cs`)

**Purpose**: Creates client applications with support for loading from seed data files.

**Key Features**:
- **Seed Data Integration**: Loads client data from `internal.clients.seed.json`
- **Owner Assignment**: Assigns client apps to existing users as owners
- **Default Fallback**: Creates default client app if no seed data available
- **API Key Support**: Handles API key assignment from seed data

**Seed Data Loading**:
```csharp
private List<Dictionary<string, object>>? LoadSeedData()
{
    var seedPath = Path.Combine(Directory.GetCurrentDirectory(), "seed.data", "internal.clients.seed.json");
    
    if (File.Exists(seedPath))
    {
        var json = File.ReadAllText(seedPath);
        var data = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);
        
        var appsFromFile = new List<Dictionary<string, object>>();
        foreach (var x in data)
        {
            var clientCode = x.GetValueOrDefault("ClientCode")?.ToString() 
                ?? x.GetValueOrDefault("ClientName", "client-app")?.ToString() 
                ?? "client-app";
            
            var app = new Dictionary<string, object>
            {
                { "name", x.GetValueOrDefault("ClientName", "Client App") },
                { "code", clientCode.Trim().ToLower() },
                { "description", x.GetValueOrDefault("Description", "Client application") },
                { "isVerified", true },
                { "isActive", true },
                { "status", "active" }
            };
            
            if (x.TryGetValue("ApiKey", out var apiKey) && apiKey != null)
            {
                app["apiKey"] = apiKey;
            }
            
            appsFromFile.Add(app);
        }
        return appsFromFile;
    }
    
    return null;
}
```

## Seed Data Structure

### Directory Layout

```
static.content/seed.data/
├── default.tenant.seed.json              # Default tenant configuration
├── default.tenant.seed.sample.json       # Sample tenant template
├── system.admin.seed.json                # System administrator user
├── system.admin.seed.sample.json         # Sample admin template
├── default.clientapps.seed.json          # Default client applications
├── default.clientapps.seed.sample.json   # Sample client templates
└── platform.info.json                   # Platform information and branding
```

#### Default Client Apps (`default.clientapps.seed.json`)

**File Location**: `static.content/seed.data/default.clientapps.seed.json`

**Purpose**: Creates default client applications for API access and authentication.

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

**Note**: Add your client application data to the `default.clientapps.seed.json` file.
```

## Configuration System

### Configuration Manager (`Config/Config.cs`)

Provides centralized configuration management with support for both environment variables and JSON configuration:

```csharp
public static class Config
{
    // Service Configuration
    public static string ServiceName => GetConfigValue("SERVICE_NAME", "Service:SERVICE_NAME") ?? "service-skeleton";
    public static string NodeEnv => GetConfigValue("NODE_ENV", "Service:NODE_ENV") ?? "development";
    public static int Port => int.TryParse(GetConfigValue("PORT", "Service:PORT"), out var port) ? port : 3000;
    
    // Database Configuration
    public static string DbDialect => GetConfigValue("DB_DIALECT", "Database:DB_DIALECT") ?? "postgres";
    public static string DbHost => GetConfigValue("DB_HOST", "Database:DB_HOST") ?? "localhost";
    public static int DbPort => int.TryParse(GetConfigValue("DB_PORT", "Database:DB_PORT"), out var dbPort) ? dbPort : 5432;
    public static string DbName => GetConfigValue("DB_NAME", "Database:DB_NAME") ?? "service_skeleton";
    
    // Seeding Configuration
    public static bool AutoSeedOnStartup => bool.TryParse(_configuration?["AUTO_SEED_ON_STARTUP"], out var autoSeed) ? autoSeed : true;
    public static int SeedUserCount => int.TryParse(_configuration?["SEED_USER_COUNT"], out var count) ? count : 2;
}
```

### Multi-Database Support

The service supports three database providers:

#### PostgreSQL
```csharp
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(connectionString));
```

#### MySQL
```csharp
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
```

#### SQL Server
```csharp
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(connectionString));
```

### Duplicate Handling

**Two-Level Approach**:
1. **Batch Level**: Attempt batch insertion first for performance
2. **Individual Level**: Fall back to individual inserts when duplicates detected

**Benefits**:
- **Performance**: Batch operations are significantly faster
- **Resilience**: Graceful handling of partial duplicates
- **Logging**: Detailed feedback on duplicate handling


#### Database Connection Validation
```csharp
public async Task<bool> CheckDatabaseConnectionAsync()
{
    try
    {
        await _context.Database.ExecuteSqlRawAsync("SELECT 1");
        Console.WriteLine("[OK] Database connection successful");
        return true;
    }
    catch (Exception error)
    {
        Console.WriteLine($"[ERROR] Database connection failed: {error.Message}");
        return false;
    }
}
```

#### Duplicate Entry Handling
- **Batch Level**: Detects duplicate errors and switches to individual processing
- **Individual Level**: Skips duplicates and continues with remaining records
- **Logging**: Provides detailed feedback on duplicate handling

#### Dependency Validation
- **User Seeder**: Checks for existing tenants before user creation
- **Role Seeder**: Validates user existence before role assignment
- **Client App Seeder**: Ensures users exist before creating client apps

### Logging Strategy

**Console-Based Logging** with structured messages:
- `[INFO]` - General information and progress updates
- `[WARN]` - Warnings about duplicates or missing data
- `[ERROR]` - Critical errors that prevent seeding
- `[OK]` - Successful completion messages

**Example Log Output**:
```
[INFO] Using B2B mode
[INFO] Will create roles: Tenant, Organization, User

[SEED] Seeding tenants...
[INFO] 0 tenant(s) already exist, skipping seed
Inserted batch of 1 records. Total created: 1
Successfully seeded 1 tenant(s)

[SEED] Seeding users...
[INFO] Assigning users to 1 tenant(s)
Inserted batch of 2 records. Total created: 2
Successfully seeded 2 users

[OK] Seeding completed!
```

## Integration with Application Startup

### Startup Pipeline

The seeding is integrated into the ASP.NET Core startup pipeline:

```csharp
public static async Task Main(string[] args)
{
    // Setup database
    DatabaseConnector.Setup();
    
    // Build web application
    var builder = WebApplication.CreateBuilder(args);
    
    // Register services and DbContext
    var connectionString = Config.Config.GetConnectionString();
    builder.Services.AddDbContext<DatabaseContext>(options => 
        options.UseDatabase(connectionString));
    
    var app = builder.Build();
    
    // Configure pipeline
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
    
    // Use seeder extension method
    app.UseSeeder();
    
    await app.RunAsync();
}
```

### Extension Method Integration

```csharp
public static WebApplication UseSeeder(this WebApplication app)
{
    var serviceProvider = app.Services;
    var isDevEnvironment = app.Environment.IsDevelopment();
    var isTestEnvironment = app.Environment.IsEnvironment("Test");
    
    Seeder.SeedAsync(serviceProvider, isDevEnvironment, isTestEnvironment).Wait();
    return app;
}
```

## Security Considerations

### Password Security
- **BCrypt Hashing**: All passwords are hashed using BCrypt with automatic salt generation
- **Secure Defaults**: Default passwords are complex and should be changed immediately
- **No Plain Text Storage**: Passwords are never stored in plain text

### Data Validation
- **Model Validation**: Entity Framework model validation ensures data integrity
- **Required Fields**: Critical fields are marked as required with appropriate constraints
- **Length Limits**: String fields have appropriate maximum length constraints

### Environment Separation
- **Configuration Isolation**: Different configurations for development, test, and production
- **Seed Data Separation**: Sample files prevent accidental use of production data
- **Database Isolation**: Support for different databases per environment

## Monitoring and Verification

### Health Checks

The service includes built-in health check capabilities:

```csharp
// In Program.cs
builder.Services.AddHealthChecks();

// In pipeline
app.MapHealthChecks("/health");
```

### Database Verification

**Connection Testing**:
```csharp
var seeder = new DatabaseSeeder(dbContext);
var isConnected = await seeder.CheckDatabaseConnectionAsync();
```

**Data Verification**:
- Count existing records before seeding
- Verify foreign key relationships
- Validate data integrity after seeding

### Swagger Integration

Development environment includes Swagger for API testing:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

## Best Practices

### Development
- **Use Sample Files**: Always use `.sample.json` files as templates
- **Test Locally**: Verify seeding process in development environment
- **Monitor Logs**: Watch console output for seeding progress and errors
- **Database Cleanup**: Reset database between seeding tests

### Production
- **Secure Configuration**: Use environment variables for sensitive data
- **Backup Strategy**: Implement database backups before seeding
- **Monitoring**: Set up logging and monitoring for production seeding
- **Gradual Rollout**: Test seeding on staging environment first

### Maintenance
- **Regular Updates**: Keep seed data current with business requirements
- **Performance Monitoring**: Monitor seeding performance and optimize batch sizes
- **Data Auditing**: Regularly audit seeded data for accuracy
- **Version Control**: Track changes to seed data files

## Troubleshooting

### Common Issues

1. **Database Connection Failures**
   - Verify connection string configuration
   - Check database server availability
   - Validate credentials and permissions
   - Ensure database exists

2. **Seeding Failures**
   - Check Entity Framework model configuration
   - Verify foreign key relationships
   - Review batch size settings
   - Examine duplicate handling logic

3. **Performance Issues**
   - Adjust batch size based on available memory
   - Monitor database connection pool
   - Check for database locks or contention
   - Optimize database indexes

4. **Data Integrity Issues**
   - Validate seed data file formats
   - Check for circular dependencies
   - Verify required field constraints
   - Review foreign key relationships

### Diagnostic Steps

1. **Enable Detailed Logging**:
   ```json
   "Logging": {
     "LogLevel": {
       "Microsoft.EntityFrameworkCore": "Information"
     }
   }
   ```

2. **Test Database Connection**:
   ```csharp
   var seeder = new DatabaseSeeder(context);
   await seeder.CheckDatabaseConnectionAsync();
   ```

3. **Verify Seed Data Files**:
   - Check JSON syntax validity
   - Validate required fields presence
   - Ensure proper file encoding (UTF-8)

4. **Monitor Resource Usage**:
   - Check memory consumption during batch operations
   - Monitor database connection usage
   - Watch for memory leaks in long-running operations

### Architectural Approach

The .NET B2B Seeding Service emphasizes:
- **Scalability**: Designed for high-volume B2B data seeding
- **Performance**: Optimized batch processing with intelligent duplicate handling
- **Flexibility**: Multi-database support and configurable batch sizes
- **Reliability**: Comprehensive error handling and recovery mechanisms
- **Modularity**: Reusable seeder components with inheritance hierarchy

This approach makes it ideal for B2B scenarios requiring large-scale data seeding with high performance and reliability requirements.