using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace shala.api.startup.configurations;

public static class BuilderExtensions
{
    public static WebApplicationBuilder AddConfigs(this WebApplicationBuilder builder)
    {
        builder.SetupConfigSource();

        builder.SetupLogging();

        // Event messaging
        builder.SetupEventMessaging();
        builder.RegisterEventPublishers();

        builder.SetupCache();

        builder.RegisterServices();
        builder.RegisterValidators();
        builder.RegisterControllers();
        builder.RegisterModules();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.SetupSwagger();
        builder.Services.SetupJsonSerialization();

        builder.Services.AddCors(Options =>
        {
            Options.AddDefaultPolicy(builder =>
            {
                builder.AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader();
            });
        });

        builder.Services.AddHealthChecks()
            .AddCheck("Self", () => HealthCheckResult.Healthy());

        // builder.Services.AddAntiforgery(options =>
        // {
        //     options.HeaderName = "X-CSRF-TOKEN"; // Specify the header name for the token
        // });

        builder.SetupUserAuth();
        builder.SetupOpenTelemetry();
        builder.AddScheduler();
        builder.SetupDatabases();

        return builder;
    }

}
