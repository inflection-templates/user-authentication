using System.Reflection;
using shala.api.domain.types;
using shala.api.startup;

namespace shala.api.modules.communication;

public class EmailService : IEmailService
{

    #region Constructor

    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly IHostEnvironment _env;
    private readonly IEmailProviderService _emailProviderService;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger,
        IHostEnvironment env,
        IEmailProviderService emailProviderService)
    {
        _configuration = configuration;
        _logger = logger;
        _env = env;
        _emailProviderService = emailProviderService;
    }

    #endregion

    public async Task<bool> SendPasswordResetLinkAsync(User user, string resetLink)
    {
        var emailParams = GetEmailParams_PasswordReset(user, resetLink);
        if (emailParams == null)
        {
            return false;
        }
        if (_env.IsDevelopment())
        {
            await dumpEmailBodyToFile("password-reset", emailParams.Body);
        }
        return await _emailProviderService.SendEmail(emailParams);
    }

    public async Task<bool> SendApiKeyAsync(User user, string clientAppName, string apiKey, string apiSecret)
    {
        var emailParams = GetEmailParams_ApiKey(user, clientAppName, apiKey, apiSecret);
        if (emailParams == null)
        {
            return false;
        }
        if (_env.IsDevelopment())
        {
            await dumpEmailBodyToFile("api-key", emailParams.Body);
        }
        return await _emailProviderService.SendEmail(emailParams);
    }

    public async Task<bool> SendEmailOtpAsync(User user, string otp, string purpose)
    {
        var emailParams = GetEmailParams_Otp(user, otp, purpose);
        if (emailParams == null)
        {
            return false;
        }
        if (_env.IsDevelopment())
        {
            await dumpEmailBodyToFile("otp", emailParams.Body);
        }
        return await _emailProviderService.SendEmail(emailParams);
    }

    public async Task<bool> SendEmailVerificationAsync(User user, string verificationLink)
    {
        var emailParams = GetEmailParams_EmailVerification(user, verificationLink);
        if (emailParams == null)
        {
            return false;
        }
        if (_env.IsDevelopment())
        {
            await dumpEmailBodyToFile("email-verification", emailParams.Body);
        }
        return await _emailProviderService.SendEmail(emailParams);
    }

    public async Task<bool> SendWelcomeEmailAsync(User user)
    {
        var emailParams = GetEmailParams_Welcome(user);
        if (emailParams == null)
        {
            return false;
        }
        if (_env.IsDevelopment())
        {
            await dumpEmailBodyToFile("welcome", emailParams.Body);
        }
        return await _emailProviderService.SendEmail(emailParams);
    }

    #region Privates

    private EmailParams? GetEmailParams_PasswordReset(User user, string resetLink)
    {
        var subject = "Password Reset";

        var emailFrom = this._configuration.GetSection("Email:From").Value ??
            throw new ArgumentNullException("Unable to find Email From in appsetting.json");

        var emailTo = user.Email;
        if (string.IsNullOrEmpty(emailTo))
        {
            return null;
        }

        string? emailBody = getEmailTemplateText("password.reset.html");
        if (string.IsNullOrEmpty(emailBody))
        {
            return null;
        }

        var userName = getUserName(user);

        emailBody = emailBody.Replace("{{Title}}", "Password Reset");
        emailBody = emailBody.Replace("{{UserName}}", userName);
        emailBody = emailBody.Replace("{{ResetLink}}", resetLink);
        emailBody = updatePlatformInfo(emailBody);

        if (string.IsNullOrEmpty(emailBody))
        {
            return null;
        }

        EmailParams email = new()
        {
            From = emailFrom,
            To = emailTo,
            Subject = subject,
            Body = emailBody,
            Purpose = "Reset Password",
        };
        return email;
    }

    private EmailParams? GetEmailParams_ApiKey(User user, string clientAppName, string apiKey, string apiSecret)
    {
        var subject = "Your API Key";

        var emailFrom = this._configuration.GetSection("Email:From").Value ??
            throw new ArgumentNullException("Unable to find Email From in appsetting.json");

        var emailTo = user.Email;
        if (string.IsNullOrEmpty(emailTo))
        {
            return null;
        }

        string? emailBody = getEmailTemplateText("api.key.html");
        if (string.IsNullOrEmpty(emailBody))
        {
            return null;
        }

        var userName = getUserName(user);

        emailBody = emailBody.Replace("{{Title}}", "API Key");
        emailBody = emailBody.Replace("{{UserName}}", userName);
        emailBody = emailBody.Replace("{{ApiKey}}", apiKey);
        emailBody = emailBody.Replace("{{ApiSecret}}", apiSecret);
        emailBody = emailBody.Replace("{{ClientAppName}}", clientAppName);
        emailBody = updatePlatformInfo(emailBody);

        if (string.IsNullOrEmpty(emailBody))
        {
            return null;
        }

        EmailParams email = new()
        {
            From = emailFrom,
            To = emailTo,
            Subject = subject,
            Body = emailBody,
            Purpose = "API Key",
        };
        return email;
    }

    private EmailParams? GetEmailParams_Otp(User user, string otp, string purpose)
    {
        var subject = "One Time Password";

        var emailFrom = this._configuration.GetSection("Email:From").Value ??
            throw new ArgumentNullException("Unable to find Email From in appsetting.json");

        var emailTo = user.Email;
        if (string.IsNullOrEmpty(emailTo))
        {
            return null;
        }

        string? emailBody = getEmailTemplateText("email.otp.html");
        if (string.IsNullOrEmpty(emailBody))
        {
            return null;
        }

        var userName = getUserName(user);

        emailBody = emailBody.Replace("{{Title}}", "One Time Password");
        emailBody = emailBody.Replace("{{UserName}}", userName);
        emailBody = emailBody.Replace("{{OTP}}", otp);
        emailBody = emailBody.Replace("{{Purpose}}", purpose);
        emailBody = updatePlatformInfo(emailBody);

        if (string.IsNullOrEmpty(emailBody))
        {
            return null;
        }

        EmailParams email = new()
        {
            From = emailFrom,
            To = emailTo,
            Subject = subject,
            Body = emailBody,
            Purpose = "OTP",
        };
        return email;
    }

    private EmailParams? GetEmailParams_EmailVerification(User user, string verificationLink)
    {
        var subject = "Email Verification";

        var emailFrom = this._configuration.GetSection("Email:From").Value ??
            throw new ArgumentNullException("Unable to find Email From in appsetting.json");

        var emailTo = user.Email;
        if (string.IsNullOrEmpty(emailTo))
        {
            return null;
        }

        string? emailBody = getEmailTemplateText("email.verification.html");
        if (string.IsNullOrEmpty(emailBody))
        {
            return null;
        }

        var userName = getUserName(user);

        emailBody = emailBody.Replace("{{Title}}", "Email Verification");
        emailBody = emailBody.Replace("{{UserName}}", userName);
        emailBody = emailBody.Replace("{{VerificationLink}}", verificationLink);
        emailBody = updatePlatformInfo(emailBody);

        if (string.IsNullOrEmpty(emailBody))
        {
            return null;
        }

        EmailParams email = new()
        {
            From = emailFrom,
            To = emailTo,
            Subject = subject,
            Body = emailBody,
            Purpose = "Email Verification",
        };
        return email;
    }

    private EmailParams? GetEmailParams_Welcome(User user)
    {
        var subject = "Welcome to the Platform";

        var emailFrom = this._configuration.GetSection("Email:From").Value ??
            throw new ArgumentNullException("Unable to find Email From in appsetting.json");

        var emailTo = user.Email;
        if (string.IsNullOrEmpty(emailTo))
        {
            return null;
        }

        string? emailBody = getEmailTemplateText("welcome.html");
        if (string.IsNullOrEmpty(emailBody))
        {
            return null;
        }

        var userName = getUserName(user);

        emailBody = emailBody.Replace("{{Title}}", "Welcome");
        emailBody = emailBody.Replace("{{UserName}}", userName);
        emailBody = updatePlatformInfo(emailBody);

        if (string.IsNullOrEmpty(emailBody))
        {
            return null;
        }

        EmailParams email = new()
        {
            From = emailFrom,
            To = emailTo,
            Subject = subject,
            Body = emailBody,
            Purpose = "Welcome",
        };
        return email;
    }

    private string? getEmailTemplateText(string template)
    {
        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var location = Directory.GetParent(assembly.Location)?.FullName;
        if (String.IsNullOrEmpty(location))
        {
            return null;
        }
        var templatePath = Path.Combine(location, "static.content", "email.templates", template);
        var baseTemplatePath = Path.Combine(location, "static.content", "email.templates", "base.template.html");
        var baseTemplateText = System.IO.File.ReadAllText(baseTemplatePath);
        if (string.IsNullOrEmpty(baseTemplateText))
        {
            return null;
        }
        var emailBody = System.IO.File.ReadAllText(templatePath);
        if (string.IsNullOrEmpty(emailBody))
        {
            return null;
        }
        emailBody = baseTemplateText.Replace("{{EmailContent}}", emailBody);
        return emailBody;
    }

    private static string? updatePlatformInfo(string emailBody)
    {
        var platformInfo = PlatformInfoHandler.GetPlatformInfo();
        if (platformInfo == null)
        {
            return null;
        }

        // Fill up the platform info
        emailBody = emailBody.Replace("{{Platform}}", platformInfo.Platform);
        emailBody = emailBody.Replace("{{Version}}", platformInfo.Version);
        emailBody = emailBody.Replace("{{Website}}", platformInfo.Website);
        emailBody = emailBody.Replace("{{SupportEmail}}", platformInfo.SupportEmail);
        emailBody = emailBody.Replace("{{SupportPhone}}", platformInfo.SupportPhone);
        emailBody = emailBody.Replace("{{CompanyAddress}}", platformInfo.CompanyAddress);
        emailBody = emailBody.Replace("{{PlatformLogo}}", platformInfo.PlatformLogo);
        emailBody = emailBody.Replace("{{AboutUs}}", platformInfo.AboutUs);
        emailBody = emailBody.Replace("{{PrivacyPolicy}}", platformInfo.PrivacyPolicy);
        emailBody = emailBody.Replace("{{TermsAndConditions}}", platformInfo.TermsAndConditions);
        emailBody = emailBody.Replace("{{Unsubscribe}}", platformInfo.Unsubscribe);
        emailBody = emailBody.Replace("{{ContactUs}}", platformInfo.ContactUs);
        emailBody = emailBody.Replace("{{Faq}}", platformInfo.Faq);

        for (int i = 0; i < platformInfo.SocialMediaLinks.Count; i++)
        {
            var key = platformInfo.SocialMediaLinks.ElementAt(i).Key;
            var value = platformInfo.SocialMediaLinks.ElementAt(i).Value;
            emailBody = emailBody.Replace($"{{SocialMediaLink_{key}}}", value);
        }
        for (int i = 0; i < platformInfo.SocialMediaIcons.Count; i++)
        {
            var key = platformInfo.SocialMediaIcons.ElementAt(i).Key;
            var value = platformInfo.SocialMediaIcons.ElementAt(i).Value;
            emailBody = emailBody.Replace($"{{SocialMediaIcon_{key}}}", value);
        }

        var currentYear = DateTime.Now.Year.ToString();
        emailBody = emailBody.Replace("{{CurrentYear}}", currentYear);

        return emailBody;
    }

    private string getUserName(User user)
    {
        var username = user.FirstName ?? "" + " " + user.LastName ?? "";
        username = username.Trim();
        username = string.IsNullOrEmpty(username) ? "User" : username;
        return username;
    }

    private async Task<bool> dumpEmailBodyToFile(string title, string emailBody)
    {
        var location = Directory.GetParent(Assembly.GetExecutingAssembly().Location)?.FullName;
        if (String.IsNullOrEmpty(location))
        {
            return false;
        }
        var tempPath = Path.Combine(location, "temp", "email");
        if (!Directory.Exists(tempPath))
        {
            Directory.CreateDirectory(tempPath);
        }
        var timeStr = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
        var fileName = $"{title}-email-body-{timeStr}.html";
        var emailBodyPath = Path.Combine(location, fileName);
        await System.IO.File.WriteAllTextAsync(emailBodyPath, emailBody);
        return true;
    }

    #endregion

}
