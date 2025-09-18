using SendWithBrevo;

namespace shala.api.modules.communication;

public class BrevoEmailService : IEmailProviderService
{
    #region Construction

    private readonly IConfiguration _configuration;
    private readonly BrevoClient _client;

    public BrevoEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        var apiKey = _configuration.GetSection("Email:Brevo:ApiKey").Value ??
            throw new ArgumentNullException("Unable to find Brevo Email Service Api Key in appsetting.json");
        _client = new BrevoClient(apiKey);
    }

    #endregion

    #region Public methods

    public async Task<bool> SendEmail(EmailParams emailParams)
    {
        try {
            var sent = await this._client.SendAsync(
                new Sender("Shala", emailParams.From),
                new List<Recipient> { new Recipient(emailParams.ToName, emailParams.To) },
                emailParams.Subject,
                emailParams.Body,
                true
            );
            return sent;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    #endregion

}
