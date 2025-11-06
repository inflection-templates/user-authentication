using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace shala.api.modules.communication;

public class TwilioSmsService: ISmsProviderService
{

    private readonly IConfiguration _configuration;

    public TwilioSmsService(IConfiguration configuration)
    {
        _configuration = configuration;
        string? accountSid = _configuration.GetSection("SMS:Twilio:AccountSid").Value;
        string? authToken = _configuration.GetSection("SMS:Twilio:AuthToken").Value;
        TwilioClient.Init(accountSid, authToken);
    }

    public async Task<bool> SendSMS(string toPhone, string message)
    {
        try
        {
            var from = _configuration.GetSection("SMS:Twilio:FromPhoneNumber").Value;
            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(from),
                to: new PhoneNumber(toPhone)
            );
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return false;
        }
    }

    public async Task<bool> SendWhatsAppMessage(string toPhone, string message)
    {
        try
        {
            var from = _configuration.GetSection("SMS:Twilio:FromPhoneNumber").Value;
            var messageResource = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(from),
                to: new PhoneNumber(toPhone)
            );
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return false;
        }
    }

}
