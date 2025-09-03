using System.Diagnostics;
using shala.api.modules.cache;

namespace shala.api.services;

public class BaseService<TService>
{
    protected readonly IConfiguration _configuration;
    protected readonly ICacheService _cacheService;
    protected readonly ILogger<TService> _logger;
    protected static readonly ActivitySource _activitySource = new("shala.api.services");
    protected bool CacheEnabled = false;
    protected bool TracingEnabled = false;

    public BaseService(IConfiguration configuration, ILogger<TService> logger, ICacheService cacheService)
    {
        _configuration = configuration;
        _logger = logger;
        _cacheService = cacheService;
        CacheEnabled = _configuration.GetValue<bool>("Cache:Enabled");
        var telemetryEnabled = _configuration.GetValue<bool>("Telemetry:Enabled");
        var tracingEnabled = _configuration.GetValue<bool>("Telemetry:Tracing:Enabled");
        TracingEnabled = telemetryEnabled && tracingEnabled;
    }

    public async Task<T> TraceAsync<T>(
    string spanName,
    Func<Task<T>> action)
    {
        if (!TracingEnabled)
        {
            return await action();
        }
        using (var activity = _activitySource.StartActivity(spanName))
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                activity?.SetTag("error", true);
                activity?.SetTag("exception.message", ex.Message);
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }

}
