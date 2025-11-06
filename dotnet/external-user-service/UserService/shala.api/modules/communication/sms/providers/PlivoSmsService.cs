using Plivo;

namespace shala.api.modules.communication;

public class PlivoSmsService: ISmsProviderService
{

    private readonly IConfiguration _configuration;

    public PlivoSmsService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> SendSMS(string toPhone, string message)
    {
        try
        {
            var from = _configuration.GetSection("Messaging:FromPhoneNumber").Value;
            var authId = _configuration.GetSection("Messaging:Plivo:AuthId").Value;
            var authToken = _configuration.GetSection("Messaging:Plivo:AuthToken").Value;
            await Task.Run(() =>
            {
                var api = new PlivoApi(authId, authToken);
                var response = api.Message.Create(
                    src: from,
                    dst: new List<String> { toPhone },
                    text: message
                );
            });
            return true;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.ToString());
            return false;
        }
    }

    public async Task<bool> SendWhatsAppMessage(string toPhone, string message)
    {
        try
        {
            var from = "whatsapp:" + _configuration.GetSection("Messaging:FromPhoneNumber").Value;
            var authId = _configuration.GetSection("Messaging:Plivo:AuthId").Value;
            var authToken = _configuration.GetSection("Messaging:Plivo:AuthToken").Value;
            await Task.Run(() =>
            {
                var api = new PlivoApi(authId, authToken);
                var response = api.Message.Create(
                    src: from,
                    dst: new List<String> { "whatsapp:" + toPhone },
                    text: message
                );
            });
            return true;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.ToString());
            return false;
        }
    }
}
