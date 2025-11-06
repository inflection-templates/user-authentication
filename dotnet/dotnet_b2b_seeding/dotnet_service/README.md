# .NET Service - Database Seeding

This .NET service provides database seeding functionality similar to the Python service implementation.

## Structure

The service follows the same structure as the Python service:

```
dotnet_service/
├── Config/
│   └── Config.cs                    # Configuration manager
├── Database/
│   ├── DatabaseContext.cs          # Entity Framework DbContext
│   ├── DatabaseConnector.cs       # Database connection manager
│   ├── Models/
│   │   ├── BaseModel.cs           # Base model with common fields
│   │   ├── User.cs                # User entity
│   │   ├── Tenant.cs              # Tenant entity
│   │   ├── Role.cs                # Role entity
│   │   └── ClientApp.cs           # ClientApp entity
│   └── Seeds/
│       ├── BaseSeeder.cs          # Base seeder class
│       ├── UserSeeder.cs          # User seeder
│       ├── TenantSeeder.cs        # Tenant seeder
│       ├── RoleSeeder.cs          # Role seeder
│       ├── ClientAppSeeder.cs     # ClientApp seeder
│       └── DatabaseSeeder.cs      # Main seeder orchestrator
├── Startup/
│   └── Seeder.cs                  # Startup seeding integration
├── Program.cs                     # Application entry point
├── appsettings.json               # Configuration file
└── DotnetService.csproj           # Project file
```

## Features

- **Same Database Schema**: All tables and columns match the Python service implementation
- **Automatic Seeding**: Seeds data on application startup when enabled
- **Entity Framework Core**: Uses EF Core for database access
- **Multi-Database Support**: Supports PostgreSQL, MySQL, and SQL Server
- **Batch Insertion**: Efficient batch insertion for large datasets
- **Duplicate Handling**: Gracefully handles duplicate entries

## Configuration

### Option 1: Using appsettings.json

Add database configuration to `appsettings.json`:

```json
{
  "Database": {
    "DB_DIALECT": "postgres",
    "DB_HOST": "localhost",
    "DB_PORT": "5432",
    "DB_NAME": "service_skeleton",
    "DB_USER_NAME": "postgres",
    "DB_USER_PASSWORD": "password"
  }
}
```

### Option 2: Using Environment Variables

Set environment variables directly (recommended for seeding, same as Python/Node.js):

```bash
# Database Configuration
DB_DIALECT=postgres
DB_HOST=localhost
DB_PORT=5432
DB_NAME=service_skeleton
DB_USER_NAME=postgres
DB_USER_PASSWORD=password

# Seeding Configuration (optional - seeding is automatic by default)
# Set AUTO_SEED_ON_STARTUP=false to disable automatic seeding
SEED_USER_COUNT=2
```

**Note**: 
- Database settings can be configured in `appsettings.json` or environment variables
- **Seeding runs automatically by default** when the database is empty (no configuration needed)
- To disable automatic seeding, set `AUTO_SEED_ON_STARTUP=false` as an environment variable
- `SEED_USER_COUNT` can be set via environment variable to control how many users are created (default: 2)
- Environment variables override `appsettings.json` values for database settings

### Database Options

- **PostgreSQL**: Set `DB_DIALECT=postgres`
- **MySQL**: Set `DB_DIALECT=mysql`
- **SQL Server**: Set `DB_DIALECT=sqlserver` (or any other value)

## Database Models

All models match the Python service:

- **User**: username, email, password, firstName, lastName, phoneNumber, etc.
- **Tenant**: name, code, domain, description, industry, etc.
- **Role**: userId, roleName
- **ClientApp**: name, code, apiKey, description, ownerUserId, etc.

All models include:
- `id` (UUID, primary key, appears first in database)
- `createdAt` (DateTime)
- `updatedAt` (DateTime)

## Running the Service

1. Install dependencies:
```bash
dotnet restore
```

2. Set environment variables or configure `appsettings.json`

3. Run the application:
```bash
dotnet run
```

The service will:
1. Connect to the database
2. Create tables if they don't exist
3. Seed data if `AUTO_SEED_ON_STARTUP=true` and database is empty

## Seeding Details

- **Tenants**: Creates 1 default tenant
- **Users**: Creates 2 users by default (configurable via `SEED_USER_COUNT`)
- **Roles**: Creates 3 roles (Tenant, Organization, User)
- **ClientApps**: Creates 2 client applications

Seed data can be loaded from JSON files:
- `seed.data/system.admin.seed.json` for admin user
- `seed.data/internal.clients.seed.json` for client apps

## Notes

- The `id` column appears first in all tables (EF Core orders primary keys first)
- Column names use camelCase to match the Python/Node.js services
- Password hashing uses BCrypt (BCrypt.Net-Next)
- Seeding only runs if the database is empty (prevents duplicate data)

