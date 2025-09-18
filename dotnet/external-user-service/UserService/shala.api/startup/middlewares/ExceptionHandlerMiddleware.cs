using OpenTelemetry.Trace;
using shala.api.common;

namespace shala.api.startup;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the exception
            _logger.LogError(ex, "An unhandled exception occurred");

            // Return a custom error response
            context.Response.StatusCode = 500;
            await context.Response.WriteAsJsonAsync(new CustomResponse() {
                Status = "failure",
                Message = $"An unexpected error occurred : {ex.Message}",
                Data = null
            });
        }
    }
}
