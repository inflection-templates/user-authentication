namespace shala.api.modules.communication;

public interface IEmailProviderService
{
    Task<bool> SendEmail(EmailParams emailParams);
}
