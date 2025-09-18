
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;

namespace shala.api.startup;

public class PlatformInfo
{
    public string Platform { get; set; } = string.Empty;
    public string? Version { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string SupportEmail { get; set; } = string.Empty;
    public string SupportPhone { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;
    public string PlatformLogo { get; set; } = string.Empty;
    public Dictionary<string, string> SocialMediaLinks { get; set; } = new Dictionary<string, string>();
    public Dictionary<string, string> SocialMediaIcons { get; set; } = new Dictionary<string, string>();
    public string? AboutUs { get; set; } = string.Empty;
    public string? PrivacyPolicy { get; set; } = string.Empty;
    public string? TermsAndConditions { get; set; } = string.Empty;
    public string? Unsubscribe { get; set; } = string.Empty;
    public string? ContactUs { get; set; } = string.Empty;
    public string? Faq { get; set; } = string.Empty;

}

public static class PlatformInfoHandler
{

    public static PlatformInfo? GetPlatformInfo()
    {
        try
        {
            var cwd = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
            if (string.IsNullOrEmpty(cwd))
            {
                return null;
            }
            var platformInfoJson = Path.Combine(cwd, "static.content", "seed.data", "platform.info.json");
            var jsonStr = File.ReadAllText(platformInfoJson);
            var options = new JsonSerializerOptions()
            {
                Converters = { new JsonStringEnumConverter() }
            };
            var platformInfo = JsonSerializer.Deserialize<PlatformInfo>(jsonStr, options);
            if (platformInfo == null)
            {
                return null;
            }
            return platformInfo;
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Error getting platform info");
        }
        return null;
    }
}
