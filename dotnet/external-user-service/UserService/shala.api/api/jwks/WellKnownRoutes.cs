using Microsoft.AspNetCore.Mvc;
using shala.api.services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using shala.api.common;



namespace shala.api;

public static class WellKnownRoutes
{
    public static void Map(WebApplication app)
    {
        var router = app.MapGroup(".well-known");

        // JWKS endpoint for public key retrieval by other services
        // RFC 7517: https://tools.ietf.org/html/rfc7517
                    router.MapGet("jwks.json", (
                [FromServices] IJwtTokenService jwtTokenService,
                [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                var jwks = jwtTokenService.GenerateJwks();
                logger.LogInformation("JWKS endpoint accessed by external service at standard path");
                
                // Return the JWKS directly as JSON content, not wrapped in ResponseHandler
                // This allows other services to parse it directly as a JWKS response
                return Results.Content(jwks, "application/json");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error serving JWKS");
                return ResponseHandler.ControllerException(ex);
            }
        })
        .WithName("WellKnown.Jwks")
        .WithOpenApi()
        .Produces<IResult>(200)
        .Produces(400)
        .Produces(401);

        // Health check endpoint for JWKS service
                    router.MapGet("jwks/health", (
                [FromServices] IJwtTokenService jwtTokenService,
                [FromServices] ILogger<Program> logger) =>
        {
            try
            {
                var signingKey = jwtTokenService.GetSigningKey();
                var healthStatus = new
                {
                    Status = "Healthy",
                    KeyType = signingKey.GetType().Name,
                    KeyId = signingKey.KeyId,
                    Algorithm = "RS256",
                    Timestamp = DateTime.UtcNow
                };
                return ResponseHandler.Ok("JWKS health check successful", healthStatus);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking JWKS health");
                return ResponseHandler.ControllerException(ex);
            }
        })
        .WithName("WellKnown.JwksHealth")
        .WithOpenApi()
        .Produces<IResult>(200)
        .Produces(400)
        .Produces(401);
    }
}
