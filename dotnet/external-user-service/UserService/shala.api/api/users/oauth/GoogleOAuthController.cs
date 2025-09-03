using System.Text.Json;
using shala.api.common;
using shala.api.services;

namespace shala.api;

public class GoogleTokenResponse
{
    public string access_token { get; set; } = null!;
    public string token_type { get; set; } = null!;
    public int expires_in { get; set; } = 0;
    public string refresh_token { get; set; } = null!;
    public string scope { get; set; } = null!;
}

public class GoogleUserInfo
{
    public string id { get; set; } = null!;
    public string email { get; set; } = null!;
    public string email_verified { get; set; } = null!;
    public string name { get; set; } = null!;
    public string given_name { get; set; } = null!;
    public string family_name { get; set; } = null!;
    public string picture { get; set; } = null!;
    public string locale { get; set; } = null!;
}

public class GoogleOAuthController: BaseOAuthController
{
    public GoogleOAuthController(
                        IConfiguration configuration,
                        ILogger<UserController> logger,
                        IUserService userService,
                        IRoleService roleService,
                        IUserRoleService userRoleService,
                        IUserAuthService userAuthService,
                        IUserAuthProfileService userAuthProfileService)
    : base(
        configuration,
        logger,
        userService,
        roleService,
        userRoleService,
        userAuthService,
        userAuthProfileService)
    {
    }

    public async Task<IResult> GetProviderLink_Google(HttpContext context)
    {
        try
        {
            var url = string.Empty;
            await Task.Run(() => {
                var clientId = _configuration.GetValue<string>("OAuth:Google:ClientId");
                var redirectUri = "http%3A%2F%2Flocalhost%3A5089%2Fapi%2Fv1%2Foauth%2Fgoogle%2Fcallback";
                var scope = "openid%20profile%20email";
                var state = Helper.GenerateRandomString(10);
                url = $"https://accounts.google.com/o/oauth2/v2/auth";
                url += $"?client_id={clientId}";
                url += $"&redirect_uri={redirectUri}";
                url += $"&response_type=code";
                url += $"&scope={scope}";
                url += $"&state={state}";
                url += $"&access_type=offline";
                url += $"&include_granted_scopes=true";
            });
            return ResponseHandler.Ok("Google login link generated successfully!", new { Url = url });
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> Login(HttpContext context, string provider, string code, string state)
    {
        try
        {
            var googleAuthUrl = "https://accounts.google.com/o/oauth2/token";
            var googleClientId = _configuration.GetValue<string>("OAuth:Google:ClientId");
            var googleClientSecret = _configuration.GetValue<string>("OAuth:Google:ClientSecret");
            var redirectUri = "http://localhost:5089/api/v1/oauth/google/callback";
            var tokenUrl = $"{googleAuthUrl}?code={code}&client_id={googleClientId}&client_secret={googleClientSecret}&redirect_uri={redirectUri}&grant_type=authorization_code";

            var client = new HttpClient();
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenUrl);
            var tokenResponse = await client.SendAsync(tokenRequest);
            if (!tokenResponse.IsSuccessStatusCode)
            {
                var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                _logger.LogError($"Error getting Google OAuth token: {errorContent}");
                return ResponseHandler.Unauthorized("Error getting Google OAuth token");
            }
            var responseText = await tokenResponse.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseText))
            {
                return ResponseHandler.Unauthorized("Error getting Google OAuth token");
            }
            var tokenObject = JsonSerializer.Deserialize<GoogleTokenResponse>(responseText);
            if (tokenObject == null)
            {
                return ResponseHandler.Unauthorized("Error getting Google OAuth token");
            }
            if (string.IsNullOrEmpty(tokenObject.access_token))
            {
                return ResponseHandler.Unauthorized("Invalid or empty Google OAuth token");
            }

            var googleUserInfoUrl = "https://www.googleapis.com/oauth2/v1/userinfo";
            var infoUrl = $"{googleUserInfoUrl}?access_token={tokenObject.access_token}";
            var infoRequest = new HttpRequestMessage(HttpMethod.Get, infoUrl);
            var infoResponse = await client.SendAsync(infoRequest);
            if (!infoResponse.IsSuccessStatusCode)
            {
                var errorContent = await infoResponse.Content.ReadAsStringAsync();
                _logger.LogError($"Error getting Google OAuth user info: {errorContent}");
                return ResponseHandler.Unauthorized("Error getting Google OAuth user info");
            }
            var infoText = await infoResponse.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(infoText))
            {
                return ResponseHandler.Unauthorized("Error getting Google OAuth user info");
            }
            var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(infoText);
            if (userInfo == null)
            {
                return ResponseHandler.Unauthorized("Error getting Google OAuth user info");
            }
            if (string.IsNullOrEmpty(userInfo.email))
            {
                return ResponseHandler.Unauthorized("Invalid or empty Google OAuth user info");
            }

            var token = tokenObject.access_token;
            var email = userInfo.email;
            var name = userInfo.name;
            var firstName = userInfo.given_name;
            var lastName = userInfo.family_name;

            var (loginResponse, mfaChallengeResponse) = await signupOrLoginAsync(context, email, firstName, lastName, provider);
            if (mfaChallengeResponse != null)
            {
                return ResponseHandler.Ok("MFA challenge required", mfaChallengeResponse);
            }
            else if (loginResponse == null)
            {
                return ResponseHandler.Unauthorized("Unable to login user");
            }
            return ResponseHandler.Ok($"User with email {email} logged in successfully!", loginResponse);
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

}
