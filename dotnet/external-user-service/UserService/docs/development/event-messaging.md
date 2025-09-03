# Event Messaging

## Overview

Event messaging is used to communicate between different services in the application. This is done to decouple the services and to make them independent of each other. The event messaging in this service is implemented using the m`MassTransit` library. `MassTransit` is a open-source distributed application framework for .NET. It provides an extensive set of features for building distributed applications and can work with various message brokers like RabbitMQ, Azure Service Bus, etc.

This service acts as a publisher as well as a consumer of the messages. The service publishes the messages to the message broker and also consumes the messages from the message broker. The messages are being used to notify the other services about the changes in the data.

## Configuration

The event messaging configuration as defined in the `appsettings.json` file is as follows:

```json
    "EventMessaging": {
        "Enabled": true,
        "Publisher": true,
        "Consumer": true,
        "Provider": "InMemory",
        "RabbitMQ": {
            "Host": "rabbitmq://localhost/",
            "Port": 5672,
        },
        "AmazonSQS": {
            "AccessKey": "<your access key>",
            "SecretKey": "<your secret key>",
            "Region": "us-east-1",
            "QueueUrl": "https://sqs.us-east-1.amazonaws.com/123456789012/MyQueue"
        },
        "AzureServiceBus": {
            "ConnectionString": "Endpoint=sb://<your-servicebus-namespace>.servicebus.windows.net/;SharedAccessKeyName=<your-policy>;SharedAccessKey=<your-key>"
        },
        "InMemory": {
            "QueueSize": 100
        }
    }
```

The `Enabled` property is used to enable or disable the event messaging. The `Publisher` and `Consumer` properties are used to enable or disable the publisher and consumer respectively. The `Provider` property is used to set the event messaging provider. The `RabbitMQ`, `AmazonSQS`, `AzureServiceBus`, and `InMemory` properties are used to set the configuration for the respective event messaging providers.

## Event Messaging Interface and Providers

The event messaging code is in `/event.messaging` folder. It contains folders for publishers and consumers.
The publisher is a class which takes the given message type and publishes it to the message broker.
There are many consumers which have been arranged in a folder structure based on the message type they consume. e.g. The consumer consuming `UserSignedUp` message is `UserSignedUpConsumer`.

```csharp

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
```

## Event Messaging Injections

The event messaging (using MassTransit) is injected into the builder's service collection in file `BuilderEventMessagingExtensions.cs` in `/startup/configurations/builder.extensions` folder. The event publisher is also injected through extension method `RegisterEventPublishers` in the same file.

## Usage

This is how one can publish an event message:

```csharp
    //Publish user signed up event to message broker
    var publisher = context.RequestServices.GetRequiredService<Publisher>();
    if (publisher != null)
    {
        var eventPublished = new UserSignedUp(
            user.Id.ToString(),
            user.Email,
            user.CountryCode,
            user.PhoneNumber);
        await publisher.Publish<UserSignedUp>(eventPublished);
    }
```
