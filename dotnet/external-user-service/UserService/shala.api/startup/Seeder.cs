using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;
using shala.api.common;
using shala.api.domain.types;
using shala.api.services;
using shala.api.modules.communication;
// using shala.api.modules.cache;

namespace shala.api.startup;

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
            Log.Information("Extracted Platform Info: ", platformInfo.Platform);
            // var serialized = JsonSerializer.Serialize(platformInfo, new JsonSerializerOptions { WriteIndented = true });
            // Log.Information(serialized);
        }
        using (var scope = serviceProvider.CreateScope())
        {
            var provider = scope.ServiceProvider;
            var tenant = await seedDefaultTenant(provider);
            await addSystemRoles(provider, tenant?.Id);
            await createSystemAdmin(provider, tenant?.Id);
            await seedDefaultClientApps(provider, isDevEnvironment, isTestEnvironment);
        }
    }

    private async static Task addSystemRoles(IServiceProvider serviceProvider, Guid? tenantId)
    {
        try
        {
            Log.Information("Seeding system roles...");
            List<DefaultRoles> roles = Enum.GetValues(typeof(DefaultRoles)).Cast<DefaultRoles>().ToList();
            var rolesWithDescriptions = EnumHelper.GetEnumWithNameAndDescriptions<DefaultRoles>();
            var roleService = serviceProvider.GetRequiredService<IRoleService>();
            foreach (var (role, name, description) in rolesWithDescriptions)
            {
                var existingRole = await roleService.GetByNameAsync(name);
                if (existingRole == null)
                {
                    var model = new RoleCreateModel
                    {
                        Name        = name,
                        Code        = name,
                        Description = description,
                        IsDefaultRole = true,
                        TenantId    = tenantId
                    };
                    await roleService.CreateAsync(model);
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
        }
    }

    private async static Task<User?> createSystemAdmin(IServiceProvider serviceProvider, Guid? tenantId)
    {
        try
        {
            Log.Information("Seeding System admin...");

            var roleService = serviceProvider.GetRequiredService<IRoleService>();
            var userService = serviceProvider.GetRequiredService<IUserService>();
            var userRoleService = serviceProvider.GetRequiredService<IUserRoleService>();
            var userAuthService = serviceProvider.GetRequiredService<IUserAuthService>();
            var userAuthProfileService = serviceProvider.GetRequiredService<IUserAuthProfileService>();
            var basePath = GetSeedDataFolderPath();
            var adminSeedPath = Path.Combine(basePath, "system.admin.seed.json");
            var jsonStr = File.ReadAllText(adminSeedPath);
            var options = new JsonSerializerOptions()
            {
                Converters = { new JsonStringEnumConverter() }
            };
            var adminModel = JsonSerializer.Deserialize<UserCreateModel>(jsonStr, options);
            if (adminModel == null)
            {
                return null;
            }
            if (string.IsNullOrEmpty(adminModel.UserName) ||
                string.IsNullOrEmpty(adminModel.Password))
            {
                return null;
            }
            var role = await roleService.GetByNameAsync(DefaultRoles.SystemAdmin.ToString());
            var user = await userService.GetByUsernameAsync(adminModel.UserName);
            if (user == null)
            {
                var userCreateModel = new UserCreateModel
                {
                    UserName    = adminModel.UserName,
                    Email       = adminModel.Email,
                    FirstName   = adminModel.FirstName,
                    LastName    = adminModel.LastName,
                    Password    = adminModel.Password,
                    CountryCode = adminModel.CountryCode,
                    PhoneNumber = adminModel.PhoneNumber,
                    TenantId    = tenantId,
                };
                user = await userService.CreateAsync(userCreateModel);
                if (user == null)
                {
                    return null;
                }
                string? hashedPassword = null;
                if (!string.IsNullOrEmpty(adminModel.Password))
                {
                    hashedPassword = BCrypt.Net.BCrypt.HashPassword(adminModel.Password);
                }
                var authProfile = await userAuthProfileService.CreateUserAuthProfileAsync(user.Id, hashedPassword);
                if (authProfile == null)
                {
                    return null;
                }
                if (role != null)
                {
                    await userRoleService.AddRoleToUserAsync(user.Id, role.Id);
                }
                Log.Information("System admin account seeded successfully");
            }
            else
            {
                Log.Error("System admin account already exists");
                if (role != null && role.Id != Guid.Empty)
                {
                    var hasRole = await userRoleService.HasUserThisRoleAsync(user.Id, role.Id);
                    if (!hasRole)
                    {
                        await userRoleService.AddRoleToUserAsync(user.Id, role.Id);
                    }
                }
            }
            return user;
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
        }
        return null;
    }

    private async static Task<Tenant?> seedDefaultTenant(IServiceProvider serviceProvider)
    {
        try
        {
            Log.Information("Seeding default tenant...");

            var tenantService = serviceProvider.GetRequiredService<ITenantService>();
            var userService = serviceProvider.GetRequiredService<IUserService>();
            var roleService = serviceProvider.GetRequiredService<IRoleService>();
            var userRoleService = serviceProvider.GetRequiredService<IUserRoleService>();

            var basePath = GetSeedDataFolderPath();
            var adminSeedPath = Path.Combine(basePath, "default.tenant.seed.json");
            var jsonStr = File.ReadAllText(adminSeedPath);
            var options = new JsonSerializerOptions()
            {
                Converters = { new JsonStringEnumConverter() }
            };
            var model = JsonSerializer.Deserialize<TenantCreateModel>(jsonStr, options);
            if (model == null)
            {
                Log.Error("Tenant model is invalid or null for seeding.");
                return null;
            }
            var tenantCode = model.Code;
            string password = model.Password;

            Tenant? defaultTenant = await tenantService.GetByCodeAsync(tenantCode ?? "default");
            if (defaultTenant != null)
            {
                Log.Error("Default tenant already exists");
                return defaultTenant;
            }

            var record = await tenantService.CreateAsync(model);
            if (record == null)
            {
                Log.Error("Default tenant not created");
            }
            else
            {
                Log.Information("Default tenant seeded successfully");
            }
            return record;
            // For default tenant, there is no need for the tenant admin user to be added.
            // It is managed by the system admin.
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
        }
        return null;
    }

    private async static Task<bool> seedDefaultClientApps(
        IServiceProvider serviceProvider,
        bool isDevEnvironment = false,
        bool isTestEnvironment = false)
    {
        try{
            Log.Information("Seeding default client apps...");

            var basePath = GetSeedDataFolderPath();
            var adminSeedPath = Path.Combine(basePath, "default.clientapps.seed.json");
            var jsonStr = File.ReadAllText(adminSeedPath);
            var options = new JsonSerializerOptions()
            {
                Converters = { new JsonStringEnumConverter() }
            };
            var models = JsonSerializer.Deserialize<List<ClientAppCreateModel>>(jsonStr, options);
            if (models == null || models.Count == 0)
            {
                return false;
            }

            var clientAppService = serviceProvider.GetRequiredService<IClientAppService>();
            var apiKeyService = serviceProvider.GetRequiredService<IApiKeyService>();
            var emailService = serviceProvider.GetRequiredService<IEmailService>();
            var userService = serviceProvider.GetRequiredService<IUserService>();
            var userRoleService = serviceProvider.GetRequiredService<IUserRoleService>();

            var owner = await userService.GetByUsernameAsync("admin");
            if (owner == null)
            {
                Log.Error("System admin not found");
                return false;
            }
            var ownerUserId = owner.Id;

            for (int i = 0; i < models.Count; i++)
            {
                var clientAppModel = models[i];
                if (clientAppModel == null)
                {
                    continue;
                }
                var existingClientApp = await clientAppService.GetByCodeAsync(clientAppModel.Code);
                if (existingClientApp == null)
                {
                    clientAppModel.OwnerUserId = ownerUserId;
                    var clientApp = await clientAppService.CreateAsync(clientAppModel);
                    if (clientApp == null)
                    {
                        continue;
                    }
                    var apiKey = Helper.GenerateKey(32);
                    var apiSecret = Helper.GenerateKey(32);
                    // Log api-key and secret to test service APIs running in docker container
                    // Log.Information($"API Key: {apiKey}");
                    // Log.Information($"API Secret: {apiSecret}");
                    var apiSecretHash = BCrypt.Net.BCrypt.HashPassword(apiSecret);
                    var apiKeyCreateModel = new ApiKeyCreateModel
                    {
                        ClientAppId = clientApp.Id,
                        Name = clientApp.Name + "Default API Key",
                        Description = "Default API Key",
                        ValidTill = DateTime.UtcNow.AddYears(1),
                        Key = apiKey,
                        SecretHash = apiSecretHash
                    };
                    var record = await apiKeyService.CreateAsync(apiKeyCreateModel);
                    if (record == null)
                    {
                        continue;
                    }

                    Log.Information($"Client app {clientApp.Name} created with default API key");

                    if (isDevEnvironment || isTestEnvironment)
                    {
                        Log.Information($"API Key: {apiKey}");
                        Log.Information($"API Secret: {apiSecret}");
                        Log.Information("Development environment. Storing API Key and secrets to temp folder");
                        var cwd = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
                        if (!string.IsNullOrEmpty(cwd))
                        {
                            var clientAppName = clientApp.Name.Replace(" ", "_").ToLower().Trim();
                            var filePath = Path.Combine(cwd, $"{clientAppName}_apikeys.json");
                            Log.Information($"Writing API Key and secret to file: {filePath}");
                            var o = new ApiKeySecret () {
                                ApiKey = apiKey,
                                ApiSecret = apiSecret
                            };
                            var str = JsonSerializer.Serialize(o, new JsonSerializerOptions { WriteIndented = true });
                            File.WriteAllText(filePath, str);
                        }
                    }
                    // else if (isTestEnvironment)
                    // {
                    //     Log.Information($"API Key: {apiKey}");
                    //     Log.Information($"API Secret: {apiSecret}");
                    //     Log.Information("Test environment. Storing API Key and secret in cache");
                    //     var cacheService = serviceProvider.GetRequiredService<ICacheService>();
                    //     if (cacheService != null)
                    //     {
                    //         await cacheService.SetAsync("ApiKey", apiKey);
                    //         await cacheService.SetAsync("ApiSecret", apiSecret);
                    //     }
                    // }
                    else
                    {
                        Log.Information("Production environment. Sending API Key to owner");
                        if (owner.Email != null)
                        {
                            var sent = await emailService.SendApiKeyAsync(owner, clientApp.Name, apiKey, apiSecret);
                            if (sent)
                            {
                                Log.Information("API Key successfully sent to owner");
                            }
                        }
                    }
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex.Message);
        }
        return false;
    }

}
