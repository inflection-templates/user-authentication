using MassTransit;

namespace shala.api.eventmessaging;

public class UserSignedUpConsumer : IConsumer<UserSignedUp>
{
    private readonly ILogger<UserSignedUpConsumer> _logger;

    public UserSignedUpConsumer(ILogger<UserSignedUpConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<UserSignedUp> context)
    {
        var userSignedUp = context.Message;

        // Process the event
        var userId = userSignedUp.UserId;
        var email = userSignedUp.Email;
        var countryCode = userSignedUp.CountryCode;
        var phoneNumber = userSignedUp.PhoneNumber;
        _logger.LogInformation($"User signed up: {userId}, {email}, {countryCode}, {phoneNumber}");

        await Task.CompletedTask;
    }
}
