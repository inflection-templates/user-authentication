
using shala.api.common;
using shala.api.services;

namespace shala.api.startup;

public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    private readonly List<string> _excludedPaths = [
        "/",
        "/health",
        "/health-check",
        "/metrics",
        "/hangfire",
        "/docs",

        "/api/v1/file-resources/download-public",

        "/api/v1/oauth/google/challenge",
        "/api/v1/oauth/google/callback",

        "/api/v1/oauth/github/challenge",
        "/api/v1/oauth/github/callback",

        "/api/v1/oauth/gitlab/challenge",
        "/api/v1/oauth/gitlab/callback",

        "/api/v1/oauth/twitter/callback",
        "/api/v1/oauth/facebook/callback",
        "/api/v1/oauth/linkedin/callback",
        // Add more paths here
    ];

    public ApiKeyAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Exclude paths from API key authentication
        var path = context.Request.Path.Value;
        if (!string.IsNullOrEmpty(path) && _excludedPaths.Contains(path.ToLower()))
        {
            await _next(context);
            return;
        }

        try
        {
            // Extract API key and secret from headers
            if (!context.Request.Headers.TryGetValue("X-Api-Key", out var extractedApiKey))
            {
                throw new MissingApiKeyException();
            }
            if (!context.Request.Headers.TryGetValue("X-Api-Secret", out var extractedApiSecret))
            {
                throw new MissingApiKeyException();
            }
            var apiKey = extractedApiKey.ToString();
            var apiSecret = extractedApiSecret.ToString();
            var apiKeyService = context.RequestServices.GetRequiredService<IApiKeyService>();
            var clientApp = await apiKeyService.ValidateAsync(apiKey, apiSecret);
            if (clientApp == null)
            {
                throw new InvalidApiKeyException();
            }
            context.Items["ClientAppId"] = clientApp.Id;
            context.Items["ClientAppName"] = clientApp.Name;
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync(ex.Message);
            return;
        }

        // Continue to the next middleware if API key is valid
        await _next(context);
    }
}
