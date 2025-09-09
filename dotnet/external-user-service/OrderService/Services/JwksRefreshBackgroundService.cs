using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OrderService.Services
{
    public class JwksRefreshBackgroundService : BackgroundService
    {
        private readonly IJwtAuthenticationService _jwtService;
        private readonly ILogger<JwksRefreshBackgroundService> _logger;
        private readonly TimeSpan _refreshInterval = TimeSpan.FromMinutes(5);

        public JwksRefreshBackgroundService(
            IJwtAuthenticationService jwtService,
            ILogger<JwksRefreshBackgroundService> logger)
        {
            _jwtService = jwtService;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("JWKS Refresh Background Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _jwtService.RefreshKeysAsync();
                    _logger.LogDebug("JWKS keys refreshed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error refreshing JWKS keys in background service");
                }

                await Task.Delay(_refreshInterval, stoppingToken);
            }

            _logger.LogInformation("JWKS Refresh Background Service stopped");
        }
    }
}
