namespace shala.api;

public static class ControllerInjector
{
    public static void Register(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<RoleController>();
        builder.Services.AddScoped<UserController>();
        builder.Services.AddScoped<UserRoleController>();
        builder.Services.AddScoped<UserAuthController>();
        builder.Services.AddScoped<MfaAuthController>();
        builder.Services.AddScoped<BaseOAuthController>();
        builder.Services.AddScoped<CommonOAuthController>();
        builder.Services.AddScoped<GoogleOAuthController>();
        builder.Services.AddScoped<GitHubOAuthController>();
        builder.Services.AddScoped<GitLabOAuthController>();
        builder.Services.AddScoped<ClientAppController>();
        builder.Services.AddScoped<ApiKeyController>();
        builder.Services.AddScoped<TenantController>();
        builder.Services.AddScoped<FileResourceController>();
    }
}
