using System.Text.Json;
using MassTransit;

namespace shala.api.eventmessaging;

public class Publisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<Publisher> _logger;

    public Publisher(IPublishEndpoint publishEndpoint, ILogger<Publisher> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Publish<T>(T message) where T : class
    {
        await _publishEndpoint.Publish(message);
        var messageStr = JsonSerializer.Serialize(message, new JsonSerializerOptions { WriteIndented = true });
        _logger.LogInformation($"Event published: {messageStr}");

        await Task.CompletedTask;
    }

}
