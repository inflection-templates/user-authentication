using shala.api.domain.types;

namespace shala.api.modules.communication;

public class SmsService : ISmsService
{

    #region Constructor

    private readonly IConfiguration _configuration;
    private readonly ILogger<SmsService> _logger;
    private readonly ISmsProviderService _smsProviderService;

    public SmsService(IConfiguration configuration, ILogger<SmsService> logger, ISmsProviderService smsProviderService)
    {
        _configuration = configuration;
        _logger = logger;
        _smsProviderService = smsProviderService;
    }

    #endregion

    public async Task<bool> SendOtpAsync(User user, string countryCode, string PhoneNumber, string otp)
    {
        var username = user.FirstName ?? "" + " " + user.LastName ?? "";
        username = username.Trim();
        username = string.IsNullOrEmpty(username) ? "User" : username;
        var message = $"Dear {username}, {otp} is your OTP. This code will expire in 5 minutes. Do not share this code with anyone.";
        return await _smsProviderService.SendSMS(countryCode + PhoneNumber, message);
    }

    public async Task<bool> SendPasswordResetLinkAsync(User user, string countryCode, string PhoneNumber, string resetLink)
    {
        var username = user.FirstName ?? "" + " " + user.LastName ?? "";
        username = username.Trim();
        username = string.IsNullOrEmpty(username) ? "User" : username;
        var message = $"Dear {username}, click on the link to reset your password: {resetLink}";
        return await _smsProviderService.SendSMS(countryCode + PhoneNumber, message);
    }
}
