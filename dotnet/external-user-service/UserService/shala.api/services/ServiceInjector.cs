namespace shala.api.services;

public static class ServiceInjector
{
    public static void Register(IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IUserAuthService, UserAuthService>();
        services.AddScoped<IUserAuthProfileService, UserAuthProfileService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IApiKeyService, ApiKeyService>();
        services.AddScoped<IClientAppService, ClientAppService>();
        services.AddScoped<IFileResourceService, FileResourceService>();
    }
}
