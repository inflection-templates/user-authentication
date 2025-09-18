using shala.api.common;
using shala.api.database.interfaces;

namespace shala.api.database.relational.efcore;

public class EFCoreInjector : IDatabaseInjector
{
    public void Register(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserRepository, UserRepository>();
        builder.Services.AddScoped<IRoleRepository, RoleRepository>();
        builder.Services.AddScoped<IUserRoleRepository, UserRoleRepository>();
        builder.Services.AddScoped<IUserAuthProfileRepository, UserAuthProfileRepository>();
        builder.Services.AddScoped<ISessionRepository, SessionRepository>();
        builder.Services.AddScoped<IOtpRepository, OtpRepository>();
        builder.Services.AddScoped<ITenantRepository, TenantRepository>();
        builder.Services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
        builder.Services.AddScoped<IClientAppRepository, ClientAppRepository>();
        builder.Services.AddScoped<IFileResourceRepository, FileResourceRepository>();
    }
}
