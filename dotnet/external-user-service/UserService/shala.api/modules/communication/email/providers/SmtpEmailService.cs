using System.Net;
using System.Net.Mail;

namespace shala.api.modules.communication;

public class SmtpEmailService : IEmailProviderService
{
    #region Construction

    private readonly IConfiguration _configuration;

    public SmtpEmailService(IConfiguration configuration)
    {
        this._configuration = configuration;
    }

    #endregion

    #region Public methods

    public async Task<bool> SendEmail(EmailParams emailParams)
    {
        try {
            string smtpServer = this._configuration.GetSection("Email:SMTP:Host").Value ?? throw new ArgumentNullException("Unable to find Email SMTP server host in appsetting.json");
            int smtpPort = int.Parse(_configuration.GetSection("Email:SMTP:Port").Value ?? throw new ArgumentNullException("Unable to find Email SMTP server port in appsetting.json"));
            string smtpUsername = _configuration.GetSection("Email:SMTP:Username").Value ?? throw new ArgumentNullException("Unable to find Email SMTP server username in appsetting.json");
            string smtpPassword = _configuration.GetSection("Email:SMTP:Password").Value ?? throw new ArgumentNullException("Unable to find Email SMTP server password in appsetting.json");

            string recipientEmail = emailParams.To;
            string subject = emailParams.Subject;
            string body = emailParams.Body;

            await Task.Run(() =>
            {
                var smtpClient = new SmtpClient(smtpServer, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword)
                };
                MailMessage mailMessage = new MailMessage(smtpUsername, recipientEmail)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                smtpClient.Send(mailMessage);

                Console.WriteLine("Email sent successfully.");
            });

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    #endregion

}
