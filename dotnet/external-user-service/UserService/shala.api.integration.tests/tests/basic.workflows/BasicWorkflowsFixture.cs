namespace shala.api.integration.tests;

public class BasicWorkflowsFixture : IDisposable
{
    internal ShalaWebApplicationFactory Factory { get; private set; }
    internal HttpClient Client { get; private set; }

    public BasicWorkflowsFixture()
    {
        Factory = new ShalaWebApplicationFactory();
        Client = Factory.CreateClient();

        var apiKey = TestDataCache.Get<string>("ApiKey");
        var apiSecret = TestDataCache.Get<string>("ApiSecret");
        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            TestUtils.PopulateApiKeyAndSecret();
            apiKey = TestDataCache.Get<string>("ApiKey");
            apiSecret = TestDataCache.Get<string>("ApiSecret");
            Console.WriteLine($"ApiKey: {apiKey}");
            Console.WriteLine($"ApiSecret: {apiSecret}");
        }
    }

    public void Dispose()
    {
        Client.Dispose();
        Factory.Dispose();
    }
}
