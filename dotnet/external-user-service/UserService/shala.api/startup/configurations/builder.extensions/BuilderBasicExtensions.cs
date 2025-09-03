using Microsoft.AspNetCore.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace shala.api.startup.configurations;

public static class BuilderBasicExtensions
{
    public static WebApplicationBuilder SetupConfigSource(this WebApplicationBuilder builder)
    {

        // Clear default configuration sources
        builder.Configuration.Sources.Clear();

        var isTestEnvironment = builder.Environment.IsEnvironment("Test");
        if (!isTestEnvironment)
        {
            // Explicitly load configuration from appsettings.json and other files
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory()) // Set base path to the current directory
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)  // Default config file
                .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true)  // Local settings
                .AddEnvironmentVariables();  // Add environment variables

                //Note: You can also add command line arguments to the configuration as below
                //.AddCommandLine(args);  // Add command-line arguments
        }
        else
        {
            // Load configuration from appsettings.json for test environment
            builder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory()) // Set base path to the current directory
                .AddJsonFile("appsettings.test.json", optional: true, reloadOnChange: true)  // Local settings
                .AddEnvironmentVariables();  // Add environment variables
        }

        return builder;
    }

    public static IServiceCollection SetupJsonSerialization(this IServiceCollection services)
    {
        services.Configure<JsonOptions>(options => {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }

}

