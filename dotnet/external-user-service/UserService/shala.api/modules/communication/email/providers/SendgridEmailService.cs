using SendGrid.Helpers.Mail;
using SendGrid;

namespace shala.api.modules.communication;

public class SendgridEmailService : IEmailProviderService
{
    #region Construction

    private readonly IConfiguration _configuration;
    private readonly SendGridClient _client;

    public SendgridEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        var apiKey = _configuration.GetSection("Email:Sendgrid:ApiKey").Value ??
            throw new ArgumentNullException("Unable to find Sendgrid Email Service Api Key in appsetting.json");
        _client = new SendGridClient(apiKey);
    }

    #endregion

    #region Public methods

    public async Task<bool> SendEmail(EmailParams emailParams)
    {
        try {
            var from = new EmailAddress(emailParams.From, "Deft-Source");
            var to = new EmailAddress(emailParams.To);
            var textContent = emailParams.Body;
            var msg = MailHelper.CreateSingleEmail(from, to, emailParams.Subject, textContent, emailParams.Body);
            var response = await _client.SendEmailAsync(msg);
            Console.WriteLine(response.StatusCode);
            return response.StatusCode == System.Net.HttpStatusCode.Accepted;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    #endregion

}
