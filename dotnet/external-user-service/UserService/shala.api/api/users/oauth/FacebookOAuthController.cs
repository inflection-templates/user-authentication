using System.Text.Json;
using shala.api.common;
using shala.api.services;

namespace shala.api;

public class FacebookTokenResponse
{
    public string access_token { get; set; } = null!;
    public string token_type { get; set; } = null!;
    public int expires_in { get; set; } = 0;
}

public class FacebookUserInfo
{
    public string id { get; set; } = null!;
    public string email { get; set; } = null!;
    public string name { get; set; } = null!;
    public string first_name { get; set; } = null!;
    public string last_name { get; set; } = null!;
    public FacebookPicture? picture { get; set; }
    public string locale { get; set; } = null!;
    public string timezone { get; set; } = null!;
    public string verified { get; set; } = null!;
}

public class FacebookPicture
{
    public FacebookPictureData? data { get; set; }
}

public class FacebookPictureData
{
    public int height { get; set; }
    public int width { get; set; }
    public bool is_silhouette { get; set; }
    public string url { get; set; } = null!;
}

public class FacebookOAuthController: BaseOAuthController
{
    public FacebookOAuthController(
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

    public async Task<IResult> GetProviderLink_Facebook(HttpContext context)
    {
        try
        {
            var url = string.Empty;
            await Task.Run(() => {
                var clientId = _configuration.GetValue<string>("OAuth:Facebook:AppId");
                var redirectUri = _configuration.GetValue<string>("OAuth:Facebook:RedirectUri");
                var scope = "email,public_profile";
                var state = Helper.GenerateRandomString(10);
                
                url = $"https://www.facebook.com/v18.0/dialog/oauth";
                url += $"?client_id={clientId}";
                url += $"&redirect_uri={Uri.EscapeDataString(redirectUri ?? "")}";
                url += $"&response_type=code";
                url += $"&scope={scope}";
                url += $"&state={state}";
            });
            return ResponseHandler.Ok("Facebook login link generated successfully!", new { Url = url });
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
            var facebookTokenUrl = "https://graph.facebook.com/v18.0/oauth/access_token";
            var facebookAppId = _configuration.GetValue<string>("OAuth:Facebook:AppId");
            var facebookAppSecret = _configuration.GetValue<string>("OAuth:Facebook:AppSecret");
            var redirectUri = _configuration.GetValue<string>("OAuth:Facebook:RedirectUri");
            
            if (string.IsNullOrEmpty(facebookAppId))
            {
                return ResponseHandler.InternalServerError("Facebook App ID not configured.");
            }
            
            if (string.IsNullOrEmpty(facebookAppSecret))
            {
                return ResponseHandler.InternalServerError("Facebook App Secret not configured.");
            }

            var tokenUrl = $"{facebookTokenUrl}?client_id={facebookAppId}&redirect_uri={Uri.EscapeDataString(redirectUri ?? "")}&client_secret={facebookAppSecret}&code={code}";

            var client = new HttpClient();
            var tokenResponse = await client.GetAsync(tokenUrl);
            if (!tokenResponse.IsSuccessStatusCode)
            {
                var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                _logger.LogError($"Error getting Facebook OAuth token: {errorContent}");
                return ResponseHandler.Unauthorized("Error getting Facebook OAuth token");
            }
            
            var responseText = await tokenResponse.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseText))
            {
                return ResponseHandler.Unauthorized("Error getting Facebook OAuth token");
            }
            
            var tokenObject = JsonSerializer.Deserialize<FacebookTokenResponse>(responseText);
            if (tokenObject == null)
            {
                return ResponseHandler.Unauthorized("Error parsing Facebook OAuth token response");
            }
            
            if (string.IsNullOrEmpty(tokenObject.access_token))
            {
                return ResponseHandler.Unauthorized("Invalid or empty Facebook OAuth token");
            }

            // Get user information from Facebook
            var facebookUserInfoUrl = "https://graph.facebook.com/v18.0/me";
            var fields = "id,email,name,first_name,last_name,picture,locale,timezone,verified";
            var infoUrl = $"{facebookUserInfoUrl}?fields={fields}&access_token={tokenObject.access_token}";
            
            var infoRequest = new HttpRequestMessage(HttpMethod.Get, infoUrl);
            var infoResponse = await client.SendAsync(infoRequest);
            if (!infoResponse.IsSuccessStatusCode)
            {
                var errorContent = await infoResponse.Content.ReadAsStringAsync();
                _logger.LogError($"Error getting Facebook OAuth user info: {errorContent}");
                return ResponseHandler.Unauthorized("Error getting Facebook OAuth user info");
            }
            
            var infoText = await infoResponse.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(infoText))
            {
                return ResponseHandler.Unauthorized("Error getting Facebook OAuth user info");
            }
            
            var userInfo = JsonSerializer.Deserialize<FacebookUserInfo>(infoText);
            if (userInfo == null)
            {
                return ResponseHandler.Unauthorized("Error parsing Facebook OAuth user info");
            }
            
            if (string.IsNullOrEmpty(userInfo.email))
            {
                return ResponseHandler.Unauthorized("Facebook account must have a verified email address");
            }

            var token = tokenObject.access_token;
            var email = userInfo.email;
            var name = userInfo.name;
            var firstName = userInfo.first_name;
            var lastName = userInfo.last_name;

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
