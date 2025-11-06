using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Serilog;
using Microsoft.AspNetCore.Authentication;
using shala.api.services;

namespace shala.api.startup.configurations;

public static class BuilderUserAuthExtensions
{

    public static WebApplicationBuilder SetupUserAuth(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization();

        // Register JWT Token Service for asymmetric key generation
        builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

        var authenticationBuilder = builder.Services.AddAuthentication(options =>
        {
            // Default to JWT Bearer Authentication for regular API endpoints
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false; //Turn on true later
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Secrets:Jwt:Issuer"] ?? "shala.api",
                ValidAudience = builder.Configuration["Secrets:Jwt:Audience"] ?? "shala.api",
                ClockSkew = TimeSpan.Zero,
                IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
                {
                    // For now, we'll use the JWT service to get the signing key
                    // In production, you might want to implement a more sophisticated key resolver
                    var jwtService = builder.Services.BuildServiceProvider().GetService<IJwtTokenService>();
                    if (jwtService != null)
                    {
                        return new List<SecurityKey> { jwtService.GetSigningKey() };
                    }
                    return new List<SecurityKey>();
                }
            };
        });

        // authenticationBuilder = addGoogleAuthentication(authenticationBuilder, builder.Configuration);
        // authenticationBuilder = addTwitterAuthentication(authenticationBuilder, builder.Configuration);
        // authenticationBuilder = addFacebookAuthentication(authenticationBuilder, builder.Configuration);
        // authenticationBuilder = addMicrosoftAuthentication(authenticationBuilder, builder.Configuration);

        // NOTE: Please note that for GitHub and GitLab OAuth, we will have custom OAuth workflow

        return builder;
    }

    private static AuthenticationBuilder addGoogleAuthentication(
        AuthenticationBuilder authenticationBuilder, ConfigurationManager configuration)
    {
        var enabled = configuration.GetValue<bool>("OAuth:Google:Enabled");
        if (enabled)
        {
            var clientId = configuration.GetValue<string>("OAuth:Google:ClientId");
            var clientSecret = configuration.GetValue<string>("OAuth:Google:ClientSecret");
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                Log.Warning("Google OAuth is enabled but ClientId or ClientSecret is missing in appsettings.json");
                return authenticationBuilder;
            }
            var callbackPath = "/api/v1/oauth/google/callback";
            authenticationBuilder.AddGoogle("google", options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.CallbackPath = callbackPath;
            });
        }
        return authenticationBuilder;
    }

    private static AuthenticationBuilder addTwitterAuthentication(
        AuthenticationBuilder authenticationBuilder, ConfigurationManager configuration)
    {
        var enabled = configuration.GetValue<bool>("OAuth:Twitter:Enabled");
        if (enabled)
        {
            var clientId = configuration.GetValue<string>("OAuth:Twitter:ConsumerAPIKey");
            var clientSecret = configuration.GetValue<string>("OAuth:Twitter:ConsumerSecret");
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                Log.Warning("Twitter OAuth is enabled but ClientId or ClientSecret is missing in appsettings.json");
                return authenticationBuilder;
            }
            var callbackPath = "/api/v1/oauth/twitter/callback";
            authenticationBuilder.AddTwitter("twitter", options =>
            {
                options.ConsumerKey = clientId;
                options.ConsumerSecret = clientSecret;
                options.CallbackPath = callbackPath;
                options.RetrieveUserDetails = true;
                options.SaveTokens = true;
            });
        }
        return authenticationBuilder;
    }

    private static AuthenticationBuilder addFacebookAuthentication(
        AuthenticationBuilder authenticationBuilder, ConfigurationManager configuration)
    {
        var enabled = configuration.GetValue<bool>("OAuth:Facebook:Enabled");
        if (enabled)
        {
            var appId = configuration.GetValue<string>("OAuth:Facebook:AppId");
            var appSecret = configuration.GetValue<string>("OAuth:Facebook:AppSecret");
            if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(appSecret))
            {
                Log.Warning("Facebook OAuth is enabled but AppId or AppSecret is missing in appsettings.json");
                return authenticationBuilder;
            }
            var callbackPath = "/api/v1/oauth/facebook/callback";
            authenticationBuilder.AddFacebook("facebook", options =>
            {
                options.ClientId = appId;
                options.ClientSecret = appSecret;
                // options.AppId = appId; //??
                // options.AppSecret = appSecret; //??
                options.CallbackPath = callbackPath;
            });
        }
        return authenticationBuilder;
    }

    private static AuthenticationBuilder addMicrosoftAuthentication(
        AuthenticationBuilder authenticationBuilder, ConfigurationManager configuration)
    {
        var enabled = configuration.GetValue<bool>("OAuth:Microsoft:Enabled");
        if (enabled)
        {
            var clientId = configuration.GetValue<string>("OAuth:Microsoft:ClientId");
            var clientSecret = configuration.GetValue<string>("OAuth:Microsoft:ClientSecret");
            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                Log.Warning("Microsoft OAuth is enabled but ClientId or ClientSecret is missing in appsettings.json");
                return authenticationBuilder;
            }
            var callbackPath = "/api/v1/oauth/microsoft/callback";
            authenticationBuilder.AddMicrosoftAccount("microsoft", options =>
            {
                options.ClientId = clientId;
                options.ClientSecret = clientSecret;
                options.CallbackPath = callbackPath;
            });
        }
        return authenticationBuilder;
    }

}
