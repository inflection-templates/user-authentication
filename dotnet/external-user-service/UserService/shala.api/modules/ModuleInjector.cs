using shala.api.modules.communication;
using shala.api.modules.storage;

namespace shala.api.modules;

public static class ModuleInjector
{
    public static void Register(IServiceCollection services)
    {
        // Email
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailProviderService, SmtpEmailService>();
        // services.AddScoped<IEmailProviderService, BrevoEmailService>();
        // services.AddScoped<IEmailProviderService, SendgridEmailService>();

        // SMS
        services.AddScoped<ISmsService, SmsService>();
        services.AddScoped<ISmsProviderService, TwilioSmsService>();
        // services.AddScoped<ISmsProviderService, PlivoSmsService>();

        // File Storage
        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IFileStorageProviderService, LocalFileStorageService>();
        // services.AddScoped<IFileStorageProviderService, AmazonS3Service>();
        // services.AddScoped<IFileStorageProviderService, AzureBlobStorageService>();
        // services.AddScoped<IFileStorageProviderService, GoogleCloudStorageService>();

        // Register other modules here
    }
}
