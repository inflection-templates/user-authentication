using Serilog;

namespace shala.api.startup.configurations;

public static class BuilderLogExtensions
{
    public static WebApplicationBuilder SetupLogging(this WebApplicationBuilder builder)
    {
        // Configure Serilog at the start to capture all logs, including from host building
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        builder.Host.UseSerilog(); //Attach Serilog to the host

        return builder;
    }

}
