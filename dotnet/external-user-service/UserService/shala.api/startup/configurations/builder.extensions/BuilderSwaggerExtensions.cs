using Microsoft.OpenApi.Models;

namespace shala.api.startup.configurations;

public static class BuilderSwaggerExtensions
{

    public static IServiceCollection SetupSwagger(this IServiceCollection services)
    {
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        var openApiInfo = new OpenApiInfo
        {
            Title = "shala.api",
            Version = "v1",
            Description = "API for Shala Learning Platform",
            Contact = new OpenApiContact
            {
                Name = "Shala Learning Platform",
                Email = "kiran.kharade@inflectionzone.com",
                Url = new Uri("https://www.shala.com"),
            },
        };

        var clientAuthSecurityDefinition = new OpenApiSecurityScheme
        {
            Name = "X-API-Key",
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Description = "API Key for client authentication"
        };

        var userAuthSecurityDefinition = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "JWT user authentication using the Bearer scheme."
        };

        var securityRequirement = new OpenApiSecurityRequirement
        {
            // For client authentication
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "ApiKey"
                    }
                },
                new string[] {}
            },
            // For user authentication
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        };

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", openApiInfo);

            // For client authentication
            c.AddSecurityDefinition("ApiKey", clientAuthSecurityDefinition);
            // For user authentication
            c.AddSecurityDefinition("Bearer", userAuthSecurityDefinition);

            c.AddSecurityRequirement(securityRequirement);
        });

        return services;

    }

}
