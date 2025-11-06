using shala.api.domain.types;

namespace shala.api.modules.communication;


public interface ISmsService
{
    Task<bool> SendOtpAsync(User user, string countryCode, string PhoneNumber, string otp);

    Task<bool> SendPasswordResetLinkAsync(User user, string countryCode, string PhoneNumber, string resetLink);
}
