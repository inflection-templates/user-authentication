using shala.api.services;

namespace shala.api.common;

public static class ClientAuthHandler
{
    public static async Task InvokeAsync(HttpContext context)
    {
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
}
