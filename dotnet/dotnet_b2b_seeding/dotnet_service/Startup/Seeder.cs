using System.Text.Json;
using System.Text.Json.Serialization;
using DotnetService.Database;
using DotnetService.Database.Models;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace DotnetService.Startup;

public static class Seeder
{
    private static string GetSeedDataFolderPath()
    {
        var cwd = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
        if (string.IsNullOrEmpty(cwd))
        {
            return string.Empty;
        }
        var staticContentPath = Path.Combine(cwd, "static.content", "seed.data");
        return staticContentPath;
    }

    public static void Seed(IServiceProvider provider, bool isDevEnvironment = false, bool isTestEnvironment = false)
    {
        Task.Run(async () =>
        {
            await SeedAsync(provider, isDevEnvironment, isTestEnvironment);
        });
    }

    public static async Task SeedAsync(
        IServiceProvider serviceProvider,
        bool isDevEnvironment = false,
        bool isTestEnvironment = false)
    {
        var platformInfo = PlatformInfoHandler.GetPlatformInfo();
        if (platformInfo != null)
        {
            Console.WriteLine($"Extracted Platform Info: {platformInfo.Platform}");
        }

        using (var scope = serviceProvider.CreateScope())
        {
            var provider = scope.ServiceProvider;
            var dbContext = provider.GetRequiredService<DatabaseContext>();
            
            var tenant = await SeedDefaultTenant(dbContext);
            var adminUser = await CreateSystemAdmin(dbContext, tenant?.Id);
            await AddSystemRoles(dbContext, tenant?.Id, adminUser?.Id);
            await SeedDefaultClientApps(dbContext, isDevEnvironment, isTestEnvironment);
        }
    }

    private async static Task AddSystemRoles(DatabaseContext context, Guid? tenantId, Guid? adminUserId)
    {
        try
        {
            Console.WriteLine("Seeding system roles...");
            
            // Default system roles: User, Organization, Tenant
            var systemRoles = new List<string> { "User", "Organization", "Tenant" };
            
            // Get admin user to assign roles to
            var adminUser = await context.Users
                .Where(u => adminUserId != null ? u.Id == adminUserId : u.Username == "admin")
                .FirstOrDefaultAsync();
            
            if (adminUser == null)
            {
                Console.WriteLine("Warning: Admin user not found. Cannot seed roles without a user.");
                return;
            }
            
            var rolesToAdd = new List<Role>();
            
            foreach (var roleName in systemRoles)
            {
                // Check if this specific role already exists for the admin user
                var existingRole = await context.Roles
                    .Where(r => r.RoleName == roleName && r.UserId == adminUser.Id)
                    .FirstOrDefaultAsync();
                
                if (existingRole == null)
                {
                    var role = new Role
                    {
                        Id = Guid.NewGuid(),
                        UserId = adminUser.Id,
                        RoleName = roleName,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    rolesToAdd.Add(role);
                    Console.WriteLine($"Adding role: {roleName}");
                }
                else
                {
                    Console.WriteLine($"Role {roleName} already exists for admin user");
                }
            }
            
            if (rolesToAdd.Count > 0)
            {
                context.Roles.AddRange(rolesToAdd);
                await context.SaveChangesAsync();
                Console.WriteLine($"Successfully seeded {rolesToAdd.Count} system roles");
            }
            else
            {
                Console.WriteLine("All system roles already exist");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding system roles: {ex.Message}");
        }
    }

    private async static Task<User?> CreateSystemAdmin(DatabaseContext context, Guid? tenantId)
    {
        try
        {
            Console.WriteLine("Seeding System admin...");
            
            // Ensure we have a tenant ID
            if (tenantId == null || tenantId == Guid.Empty)
            {
                Console.WriteLine("Tenant ID is null or empty. Attempting to retrieve default tenant.");
                var defaultTenant = await context.Tenants
                    .Where(t => t.Code == "default")
                    .FirstOrDefaultAsync();
                if (defaultTenant != null && defaultTenant.Id != Guid.Empty)
                {
                    tenantId = defaultTenant.Id;
                    Console.WriteLine($"Retrieved default tenant ID: {tenantId}");
                }
                else
                {
                    Console.WriteLine("Could not retrieve default tenant. User will be created without tenant ID.");
                }
            }
            
            var basePath = GetSeedDataFolderPath();
            var adminSeedPath = Path.Combine(basePath, "system.admin.seed.json");
            var jsonStr = File.ReadAllText(adminSeedPath);
            var options = new JsonSerializerOptions()
            {
                Converters = { new JsonStringEnumConverter() }
            };
            
            var adminModel = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonStr, options);
            if (adminModel == null)
            {
                return null;
            }
            
            var userName = adminModel.GetValueOrDefault("UserName")?.ToString();
            var password = adminModel.GetValueOrDefault("Password")?.ToString();
            
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                return null;
            }
            
            var existingUser = await context.Users
                .Where(u => u.Username == userName)
                .FirstOrDefaultAsync();
            
            User? user;
            if (existingUser == null)
            {
                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = userName,
                    Email = adminModel.GetValueOrDefault("Email")?.ToString() ?? "admin@example.com",
                    Password = hashedPassword,
                    FirstName = adminModel.GetValueOrDefault("FirstName")?.ToString(),
                    LastName = adminModel.GetValueOrDefault("LastName")?.ToString(),
                    CountryCode = adminModel.GetValueOrDefault("CountryCode")?.ToString(),
                    PhoneNumber = adminModel.GetValueOrDefault("PhoneNumber")?.ToString(),
                    TenantId = tenantId,
                    IsActive = true,
                    IsEmailVerified = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                
                context.Users.Add(user);
                await context.SaveChangesAsync();
                
                // Add User role for system admin
                var userRole = await context.Roles
                    .Where(r => r.RoleName == "User" && r.UserId == user.Id)
                    .FirstOrDefaultAsync();
                
                if (userRole == null)
                {
                    userRole = new Role
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        RoleName = "User",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    context.Roles.Add(userRole);
                    await context.SaveChangesAsync();
                }
                
                Console.WriteLine("System admin account seeded successfully");
            }
            else
            {
                user = existingUser;
                Console.WriteLine($"System admin account already exists with ID: {user.Id}, TenantId: {user.TenantId}");
                
                // Ensure User role is assigned
                var hasUserRole = await context.Roles
                    .Where(r => r.UserId == user.Id && r.RoleName == "User")
                    .AnyAsync();
                
                if (!hasUserRole)
                {
                    var userRole = new Role
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        RoleName = "User",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    context.Roles.Add(userRole);
                    await context.SaveChangesAsync();
                }
            }
            
            return user;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding system admin: {ex.Message}");
        }
        return null;
    }

    private async static Task<Tenant?> SeedDefaultTenant(DatabaseContext context)
    {
        try
        {
            Console.WriteLine("Seeding default tenant...");
            
            var basePath = GetSeedDataFolderPath();
            var tenantSeedPath = Path.Combine(basePath, "default.tenant.seed.json");
            var jsonStr = File.ReadAllText(tenantSeedPath);
            var options = new JsonSerializerOptions()
            {
                Converters = { new JsonStringEnumConverter() }
            };
            
            var model = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonStr, options);
            if (model == null)
            {
                Console.WriteLine("Tenant model is invalid or null for seeding.");
                return null;
            }
            
            var tenantCode = model.GetValueOrDefault("Code")?.ToString() ?? "default";
            
            var defaultTenant = await context.Tenants
                .Where(t => t.Code == tenantCode)
                .FirstOrDefaultAsync();
            
            if (defaultTenant != null)
            {
                Console.WriteLine($"Default tenant already exists with ID: {defaultTenant.Id}");
                return defaultTenant;
            }
            
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = model.GetValueOrDefault("Name")?.ToString() ?? "default",
                Code = tenantCode,
                Description = model.GetValueOrDefault("Description")?.ToString(),
                ContactEmail = model.GetValueOrDefault("Email")?.ToString(),
                ContactPhone = model.GetValueOrDefault("PhoneNumber")?.ToString(),
                IsActive = true,
                IsVerified = true,
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            
            context.Tenants.Add(tenant);
            await context.SaveChangesAsync();
            
            Console.WriteLine($"Default tenant seeded successfully with ID: {tenant.Id}");
            return tenant;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding default tenant: {ex.Message}");
            // Try to retrieve existing tenant as fallback
            try
            {
                var existingTenant = await context.Tenants
                    .Where(t => t.Code == "default")
                    .FirstOrDefaultAsync();
                if (existingTenant != null)
                {
                    Console.WriteLine($"Retrieved existing default tenant as fallback with ID: {existingTenant.Id}");
                    return existingTenant;
                }
            }
            catch (Exception fallbackEx)
            {
                Console.WriteLine($"Failed to retrieve existing tenant as fallback: {fallbackEx.Message}");
            }
        }
        return null;
    }

    private async static Task<bool> SeedDefaultClientApps(
        DatabaseContext context,
        bool isDevEnvironment = false,
        bool isTestEnvironment = false)
    {
        try
        {
            Console.WriteLine("Seeding default client apps...");
            
            var basePath = GetSeedDataFolderPath();
            var clientAppsSeedPath = Path.Combine(basePath, "default.clientapps.seed.json");
            var jsonStr = File.ReadAllText(clientAppsSeedPath);
            var options = new JsonSerializerOptions()
            {
                Converters = { new JsonStringEnumConverter() }
            };
            
            var models = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonStr, options);
            if (models == null || models.Count == 0)
            {
                return false;
            }
            
            var owner = await context.Users
                .Where(u => u.Username == "admin")
                .FirstOrDefaultAsync();
            
            if (owner == null)
            {
                Console.WriteLine("System admin not found");
                return false;
            }
            
            foreach (var clientAppModel in models)
            {
                if (clientAppModel == null)
                {
                    continue;
                }
                
                var appCode = clientAppModel.GetValueOrDefault("Code")?.ToString();
                if (string.IsNullOrEmpty(appCode))
                {
                    continue;
                }
                
                var existingClientApp = await context.ClientApps
                    .Where(c => c.Code == appCode)
                    .FirstOrDefaultAsync();
                
                if (existingClientApp == null)
                {
                    var clientApp = new ClientApp
                    {
                        Id = Guid.NewGuid(),
                        Code = appCode,
                        Name = clientAppModel.GetValueOrDefault("Name")?.ToString() ?? "Default Client App",
                        Description = clientAppModel.GetValueOrDefault("Description")?.ToString() ?? "",
                        OwnerUserId = owner.Id,
                        RedirectUri = clientAppModel.GetValueOrDefault("RedirectUri")?.ToString(),
                        LogoUrl = clientAppModel.GetValueOrDefault("LogoUrl")?.ToString(),
                        WebsiteUrl = clientAppModel.GetValueOrDefault("WebsiteUrl")?.ToString(),
                        PrivacyPolicyUrl = clientAppModel.GetValueOrDefault("PrivacyPolicyUrl")?.ToString(),
                        TermsOfServiceUrl = clientAppModel.GetValueOrDefault("TermsOfServiceUrl")?.ToString(),
                        IsActive = true,
                        IsVerified = false,
                        Status = "active",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    
                    context.ClientApps.Add(clientApp);
                    await context.SaveChangesAsync();
                    
                    // Generate API key (simple implementation)
                    var apiKey = Guid.NewGuid().ToString("N").Substring(0, 32);
                    clientApp.ApiKey = apiKey;
                    await context.SaveChangesAsync();
                    
                    Console.WriteLine($"Client app {clientApp.Name} created with default API key");
                    
                    if (isDevEnvironment || isTestEnvironment)
                    {
                        Console.WriteLine($"API Key: {apiKey}");
                        Console.WriteLine("Development environment. Storing API Key to temp folder");
                        var cwd = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
                        if (!string.IsNullOrEmpty(cwd))
                        {
                            var clientAppName = clientApp.Name.Replace(" ", "_").ToLower().Trim();
                            var filePath = Path.Combine(cwd, $"{clientAppName}_apikeys.json");
                            Console.WriteLine($"Writing API Key to file: {filePath}");
                            var apiKeyData = new { ApiKey = apiKey };
                            var str = JsonSerializer.Serialize(apiKeyData, new JsonSerializerOptions { WriteIndented = true });
                            File.WriteAllText(filePath, str);
                        }
                    }
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error seeding default client apps: {ex.Message}");
        }
        return false;
    }
}
