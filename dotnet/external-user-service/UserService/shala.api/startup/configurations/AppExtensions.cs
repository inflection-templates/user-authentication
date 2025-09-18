using Hangfire;
using HealthChecks.UI.Client;
// using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Serilog;
using shala.api.startup.scheduler;

namespace shala.api.startup.configurations;

public static class AppExtensions
{

    public static WebApplication UseConfigs(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlerMiddleware>();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "shala.api v1"));
        }

        if (!app.Environment.IsDevelopment())
        {
            //app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseCors();

        // Authentication and Authorization

        app.UseAuthentication();
        if (!app.Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }
        app.UseAuthorization();

        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var addresses = app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>();
            
            Log.Information("=");
            Log.Information("🎉 User Service is ready!");
            Log.Information("🌐 Service URLs:");
            
            if (addresses != null)
            {
                foreach (var address in addresses.Addresses)
                {
                    Log.Information($"   • Main Service: {address}");
                    Log.Information($"   • JWKS Endpoint: {address}/.well-known/jwks.json");
                    Log.Information($"   • Swagger UI: {address}/swagger");
                    Log.Information($"   • Health Check: {address}/health");
                }
            }
            
            Log.Information("🔐 JWT Authentication configured with JWKS");
            Log.Information("📋 Key endpoints:");
            Log.Information("   • POST /api/users/auth/login-with-password - User login");
            Log.Information("   • GET /.well-known/jwks.json - JWKS public keys");
            Log.Information("   • GET /api/users - Get users (requires auth)");
            Log.Information("=");
            Log.Information("🚀 User Service is now running and ready to accept requests!");
            Log.Information("=");
        });

        app.Lifetime.ApplicationStopping.Register(() =>
        {
            Log.Information("Application stopping");
        });

        app.Lifetime.ApplicationStopped.Register(() =>
        {
            Log.Information("Application stopped");
        });

        // app.Use(async (context, next) =>
        // {
        //     var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
        //     // Skip validation for safe HTTP methods (GET, HEAD, OPTIONS, TRACE)
        //     if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
        //     {
        //         await antiforgery.ValidateRequestAsync(context);
        //     }
        //     await next();
        // });

        //If OpenTelemetry prometheus exporter is added, this will add
        //a scaping '/metrics' endpoint to the api
        app.MapTelemetryMetricsEndpoint();

        // Hangfire dashboard setup for background jobs
        app.UseHangfireDashboard("/hangfire");

        app.ScheduleTasks();
        // Endpoints mapping for ASP.NET Core MVC
        // app.MapControllers();

        app.UseSerilogRequestLogging();

        // API key authentication middleware
        //app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

        // Endpoints mapping for ASP.NET Core Minimal APIs
        // using the AppRoutesExtensions class
        app.RegisterRoutes();

        app.MapHealthChecks("/health-check", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse // Provides a detailed response
        });

        app.UseSeeder();

        return app;
    }

}

/////////////////////////////////////////////////////////////////////////////////////////

    // public static WebApplicationBuilder RegisterRoutes(this WebApplicationBuilder builder)
    // {
    //     builder.Services.AddHealthChecks();
    //     builder.Services.AddRazorPages();
    //     builder.Services.AddSignalR();
    //     builder.Services.AddGrpc();
    //     builder.Services.AddGraphQLServer()
    //         .AddQueryType<Query>()
    //         .AddMutationType<Mutation>()
    //         .AddSubscriptionType<Subscription>()
    //         .AddType<AuthorType>()
    //         .AddType<BookType>()
    //         .AddType<AuthorReviewInputType>();
    //     return builder;
    // }

/////////////////////////////////////////////////////////////////////////////////////////
