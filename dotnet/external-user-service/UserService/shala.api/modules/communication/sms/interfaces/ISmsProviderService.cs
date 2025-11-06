namespace shala.api.modules.communication;

public interface ISmsProviderService
{
    Task<bool> SendSMS(string toPhone, string message);

    Task<bool> SendWhatsAppMessage(string toPhone, string message);
}
