using System.Text.Json;
using shala.api.domain.types;

namespace shala.api.integration.tests;

public class TestUtils
{
    public static void PopulateApiKeyAndSecret()
    {
        try
        {
            var cwd = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location);
            if (!string.IsNullOrEmpty(cwd))
            {
                var filePath = Path.Combine(cwd, "default_internal_api_client_apikeys.json");
                if (!File.Exists(filePath))
                {
                    return;
                }
                var jsonStr = File.ReadAllText(filePath);
                var obj = JsonSerializer.Deserialize<ApiKeySecret>(jsonStr);
                if (obj != null)
                {
                    var key = obj.ApiKey;
                    var secret = obj.ApiSecret;
                    TestDataCache.Set("ApiKey", key);
                    TestDataCache.Set("ApiSecret", secret);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
