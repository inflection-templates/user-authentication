using Serilog;
using shala.api.modules.cache;

namespace shala.api.startup.configurations;

public static class BuilderCacheExtensions
{
    public static WebApplicationBuilder SetupCache(this WebApplicationBuilder builder)
    {
        var enabled = builder.Configuration.GetValue<bool>("Cache:Enabled");
        if (!enabled)
        {
            return builder;
        }
        var messagingProvider = builder.Configuration.GetValue<string>("Cache:Provider");
        if (string.IsNullOrEmpty(messagingProvider))
        {
            messagingProvider = "Memory";
        }

        if (messagingProvider == "Redis")
        {
            Log.Information("Setting up Redis Cache");
            builder.Services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            Log.Information("Setting up Memory Cache");
            builder.Services.AddMemoryCache();
            builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
        }

        return builder;
    }
}
