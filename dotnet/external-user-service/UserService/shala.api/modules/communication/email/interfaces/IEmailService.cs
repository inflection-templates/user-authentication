using shala.api.domain.types;

namespace shala.api.modules.communication;

public interface IEmailService
{
    Task<bool> SendPasswordResetLinkAsync(User user, string resetLink);

    Task<bool> SendApiKeyAsync(User user, string clientAppName, string apiKey, string apiSecret);

    Task<bool> SendEmailOtpAsync(User user, string otp, string purpose);

    Task<bool> SendEmailVerificationAsync(User user, string verificationLink);

    Task<bool> SendWelcomeEmailAsync(User user);
}
