using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace shala.api.integration.tests;

[Collection("Basic workflows")]
public class AdminTests
{
    private readonly BasicWorkflowsFixture _fixture;

    public AdminTests(BasicWorkflowsFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task T01_HealthCheck()
    {
        var response = await _fixture.Client.GetAsync("/health-check");
        var responseString = await response.Content.ReadAsStringAsync();
        var responseStatusCode = response.StatusCode;
        var resObj = JsonSerializer.Deserialize<dynamic>(responseString);
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task T02_AdminLogin()
    {
        var apiKey = TestDataCache.Get<string>("ApiKey");
        var apiSecret = TestDataCache.Get<string>("ApiSecret");

        var requestUrl = "api/v1/auth/login-with-password";
        var requestBody = new {
            UserName = "admin",
            Password = "Inflection@123"
        };
        var requestContent = JsonContent.Create(requestBody);

        _fixture.Client.DefaultRequestHeaders.Add("x-api-key", apiKey);
        _fixture.Client.DefaultRequestHeaders.Add("x-api-secret", apiSecret);
        _fixture.Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _fixture.Client.PostAsync(requestUrl, requestContent);
        var responseString = await response.Content.ReadAsStringAsync();
        var res = JsonSerializer.Deserialize<LoginRequestResponse>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(res);
        Assert.NotNull(res.Data);

        var resData = res.Data;
        Assert.NotNull(resData);
        Assert.NotNull(resData.Token);
        Assert.NotEmpty(resData.Token);
        Assert.NotNull(resData.RefreshToken);
        Assert.NotEmpty(resData.RefreshToken);
        Assert.NotNull(resData.ValidTill);
        Assert.Equal(200, (int)response.StatusCode);

        TestDataCache.Set("AdminAccessToken", resData.Token);
        TestDataCache.Set("AdminRefreshToken", resData.RefreshToken);

        response.EnsureSuccessStatusCode();
    }

}
