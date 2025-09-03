using System.Security.Authentication;
using MassTransit;
using Serilog;
using shala.api.eventmessaging;

namespace shala.api.startup.configurations;

public static class BuilderEventMessagingExtensions
{
    public static WebApplicationBuilder SetupEventMessaging(this WebApplicationBuilder builder)
    {
        var enabled = builder.Configuration.GetValue<bool>("EventMessaging:Enabled");
        if (!enabled)
        {
            return builder;
        }
        var messagingProvider = builder.Configuration.GetValue<string>("EventMessaging:Provider");
        if (string.IsNullOrEmpty(messagingProvider))
        {
            messagingProvider = "InMemory";
        }

        // Add MassTransit and configure RabbitMQ
        builder.Services.AddMassTransit(x =>
        {
            // Using kebab case for endpoint names
            x.SetKebabCaseEndpointNameFormatter();
            var assembly = typeof(Program).Assembly;

            x.AddConsumers(assembly);

            if (messagingProvider == "InMemory")
            {
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            }
            else if (messagingProvider == "RabbitMQ")
            {
                var host = builder.Configuration.GetValue<string>("EventMessaging:RabbitMQ:Host");
                var username = builder.Configuration.GetValue<string>("EventMessaging:RabbitMQ:Username");
                var password = builder.Configuration.GetValue<string>("EventMessaging:RabbitMQ:Password");
                var port = builder.Configuration.GetValue<int>("EventMessaging:RabbitMQ:Port");
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(host, "/", h =>
                    {
                        h.Username(username ?? "guest");
                        h.Password(password ?? "guest");
                        h.UseSsl(ssl =>
                        {
                            ssl.Protocol = SslProtocols.Tls12;
                            ssl.ServerName = host; // Server name for certificate validation
                        });
                        cfg.ConfigureEndpoints(context);
                    });
                });
            }
            else if (messagingProvider == "AzureServiceBus")
            {
                var connectionString = builder.Configuration.GetValue<string>("EventMessaging:AzureServiceBus:ConnectionString");
                var topicName = builder.Configuration.GetValue<string>("EventMessaging:AzureServiceBus:TopicName");

                x.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(connectionString);
                    cfg.ConfigureEndpoints(context);
                });
            }
            else if (messagingProvider == "AmazonSQS")
            {
                var region = builder.Configuration.GetValue<string>("EventMessaging:AmazonSQS:Region");
                var accessKey = builder.Configuration.GetValue<string>("EventMessaging:AmazonSQS:AccessKey");
                var secretKey = builder.Configuration.GetValue<string>("EventMessaging:AmazonSQS:SecretKey");
                x.UsingAmazonSqs((context, cfg) =>
                {
                    cfg.Host(region, h =>
                    {
                        h.AccessKey(accessKey);
                        h.SecretKey(secretKey);
                    });
                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            }

        });

        return builder;
    }

    public static WebApplicationBuilder RegisterEventPublishers(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<Publisher>();
        return builder;
    }
}
