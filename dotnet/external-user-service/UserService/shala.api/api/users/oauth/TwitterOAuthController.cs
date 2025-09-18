using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using shala.api.common;
using shala.api.services;

namespace shala.api;

public class TwitterTokenResponse
{
    public string access_token { get; set; } = null!;
    public string token_type { get; set; } = null!;
    public int expires_in { get; set; } = 0;
    public string refresh_token { get; set; } = null!;
    public string scope { get; set; } = null!;
}

public class TwitterUserInfo
{
    public TwitterUserData? data { get; set; }
}

public class TwitterUserData
{
    public string id { get; set; } = null!;
    public string name { get; set; } = null!;
    public string username { get; set; } = null!;
    public string profile_image_url { get; set; } = null!;
    public bool verified { get; set; } = false;
    public string description { get; set; } = null!;
    public TwitterUserPublicMetrics? public_metrics { get; set; }
}

public class TwitterUserPublicMetrics
{
    public int followers_count { get; set; }
    public int following_count { get; set; }
    public int tweet_count { get; set; }
    public int listed_count { get; set; }
}

public class TwitterEmailInfo
{
    public TwitterEmailData? data { get; set; }
}

public class TwitterEmailData
{
    public string email { get; set; } = null!;
}

public class TwitterOAuthController: BaseOAuthController
{
    public TwitterOAuthController(
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

    public async Task<IResult> GetProviderLink_Twitter(HttpContext context)
    {
        try
        {
            var url = string.Empty;
            await Task.Run(() => {
                var clientId = _configuration.GetValue<string>("OAuth:Twitter:ClientId");
                var redirectUri = _configuration.GetValue<string>("OAuth:Twitter:RedirectUri");
                var scope = "tweet.read%20users.read%20offline.access"; // URL encoded
                var state = Helper.GenerateRandomString(10);
                var codeChallenge = GenerateCodeChallenge();
                
                url = $"https://twitter.com/i/oauth2/authorize";
                url += $"?response_type=code";
                url += $"&client_id={clientId}";
                url += $"&redirect_uri={Uri.EscapeDataString(redirectUri ?? "")}";
                url += $"&scope={scope}";
                url += $"&state={state}";
                url += $"&code_challenge={codeChallenge}";
                url += $"&code_challenge_method=S256";
            });
            return ResponseHandler.Ok("Twitter login link generated successfully!", new { Url = url });
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
            var twitterTokenUrl = "https://api.twitter.com/2/oauth2/token";
            var clientId = _configuration.GetValue<string>("OAuth:Twitter:ClientId");
            var clientSecret = _configuration.GetValue<string>("OAuth:Twitter:ClientSecret");
            var redirectUri = _configuration.GetValue<string>("OAuth:Twitter:RedirectUri");
            
            if (string.IsNullOrEmpty(clientId))
            {
                return ResponseHandler.InternalServerError("Twitter Client ID not configured.");
            }
            
            if (string.IsNullOrEmpty(clientSecret))
            {
                return ResponseHandler.InternalServerError("Twitter Client Secret not configured.");
            }

            // Prepare token request
            var tokenRequestBody = new Dictionary<string, string>
            {
                {"code", code},
                {"grant_type", "authorization_code"},
                {"client_id", clientId},
                {"redirect_uri", redirectUri ?? ""},
                {"code_verifier", "challenge"} // In production, store and retrieve the actual code verifier
            };

            var client = new HttpClient();
            
            // Add Basic Auth header
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");
            
            var tokenRequest = new FormUrlEncodedContent(tokenRequestBody);
            var tokenResponse = await client.PostAsync(twitterTokenUrl, tokenRequest);
            
            if (!tokenResponse.IsSuccessStatusCode)
            {
                var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                _logger.LogError($"Error getting Twitter OAuth token: {errorContent}");
                return ResponseHandler.Unauthorized("Error getting Twitter OAuth token");
            }
            
            var responseText = await tokenResponse.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseText))
            {
                return ResponseHandler.Unauthorized("Error getting Twitter OAuth token");
            }
            
            var tokenObject = JsonSerializer.Deserialize<TwitterTokenResponse>(responseText);
            if (tokenObject == null)
            {
                return ResponseHandler.Unauthorized("Error parsing Twitter OAuth token response");
            }
            
            if (string.IsNullOrEmpty(tokenObject.access_token))
            {
                return ResponseHandler.Unauthorized("Invalid or empty Twitter OAuth token");
            }

            // Get user information from Twitter API v2
            var userInfo = await GetTwitterUserInfo(tokenObject.access_token);
            if (userInfo == null || userInfo.data == null)
            {
                return ResponseHandler.Unauthorized("Error getting Twitter user info");
            }

            // Get user email (requires special permission)
            var email = await GetTwitterUserEmail(tokenObject.access_token);
            if (string.IsNullOrEmpty(email))
            {
                // Fallback: use Twitter username as email (not ideal but functional)
                email = $"{userInfo.data.username}@twitter.local";
                _logger.LogWarning("Twitter email not available, using username as fallback");
            }

            var name = userInfo.data.name;
            var firstName = name;
            var lastName = string.Empty;
            
            // Try to split name
            if (!string.IsNullOrEmpty(name))
            {
                var names = name.Split(' ');
                if (names.Length > 1)
                {
                    firstName = names[0];
                    lastName = string.Join(' ', names[1..]);
                }
            }

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

    #region Private Methods

    private string GenerateCodeChallenge()
    {
        // In production, you should store the code verifier and use it in the token exchange
        // For simplicity, using a fixed challenge here
        using var sha256 = SHA256.Create();
        var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes("challenge"));
        return Convert.ToBase64String(challengeBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private async Task<TwitterUserInfo?> GetTwitterUserInfo(string accessToken)
    {
        try
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            
            var userInfoUrl = "https://api.twitter.com/2/users/me?user.fields=id,name,username,profile_image_url,verified,description,public_metrics";
            var response = await client.GetAsync(userInfoUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Error getting Twitter user info: {errorContent}");
                return null;
            }
            
            var responseText = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseText))
            {
                return null;
            }
            
            return JsonSerializer.Deserialize<TwitterUserInfo>(responseText);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception getting Twitter user info: {ex.Message}");
            return null;
        }
    }

    private async Task<string?> GetTwitterUserEmail(string accessToken)
    {
        try
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");
            
            var emailUrl = "https://api.twitter.com/2/users/me?user.fields=email";
            var response = await client.GetAsync(emailUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                // Email access requires special permission, so this might fail
                return null;
            }
            
            var responseText = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseText))
            {
                return null;
            }
            
            var emailInfo = JsonSerializer.Deserialize<TwitterEmailInfo>(responseText);
            return emailInfo?.data?.email;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception getting Twitter user email: {ex.Message}");
            return null;
        }
    }

    #endregion
}
