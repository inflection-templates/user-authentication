using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OrderService.Services;
using StackExchange.Redis;

namespace OrderService.Startup
{
    public static class JwtAuthenticationConfiguration
    {
        public static IServiceCollection AddJwtAuthenticationAndAuthorization(this IServiceCollection services, IConfiguration configuration)
        {
            // Pluggable cache registration (local or redis)
            var cacheProvider = configuration["Jwt:Cache:Provider"] ?? "local";
            if (string.Equals(cacheProvider, "redis", StringComparison.OrdinalIgnoreCase))
            {
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                {
                    var cs = configuration.GetConnectionString("Redis") ?? "localhost:6379,abortConnect=false";
                    return ConnectionMultiplexer.Connect(cs);
                });
                services.AddSingleton<IJwksKeyCache, RedisJwksKeyCache>();
                Console.WriteLine("🧠 JWKS Cache Provider: Redis");
            }
            else
            {
                services.AddSingleton<IJwksKeyCache, InMemoryJwksKeyCache>();
                Console.WriteLine("🧠 JWKS Cache Provider: InMemory");
            }

            var authority = configuration["Jwt:Authority"];
            var audience = configuration["Jwt:Audience"];
            var jwksUrl = configuration["Jwt:JwksUrl"] ?? $"{authority}/.well-known/jwks.json";

            Console.WriteLine($"🔐 Configuring JWT Authentication:");
            Console.WriteLine($"   • Authority: {authority}");
            Console.WriteLine($"   • Audience: {audience}");
            Console.WriteLine($"   • JWKS URL: {jwksUrl}");

            // Build a single provider once to resolve the authentication service and reuse it in the resolver
            var providerForResolver = services.BuildServiceProvider();
            var jwtServiceForResolver = providerForResolver.GetRequiredService<IJwtAuthenticationService>();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = audience,
                        ValidAudience = audience,
                        ClockSkew = TimeSpan.FromMinutes(5),
                        IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                        {
                            return jwtServiceForResolver.GetSigningKeysAsync(kid).GetAwaiter().GetResult();
                        }
                    };
                });

            services.AddAuthorization();

            return services;
        }
    }
}


