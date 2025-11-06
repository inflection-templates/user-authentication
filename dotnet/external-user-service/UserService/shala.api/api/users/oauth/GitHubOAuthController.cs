using System.Text.Json;
using shala.api.common;
using shala.api.services;

namespace shala.api;


internal class GithubAccessTokenModel
{
    public string access_token { get; set; } = string.Empty;
    public string token_type { get; set; } = string.Empty;
    public string scope { get; set; } = string.Empty;
}

internal class GithubUser
{
    public string login { get; set; } = "";
    public int id { get; set; }
    public string node_id { get; set; } = "";
    public string avatar_url { get; set; } = "";
    public string url { get; set; } = "";
    public string html_url { get; set; } = "";
    public string subscriptions_url { get; set; } = "";
    public string organizations_url { get; set; } = "";
    public string repos_url { get; set; } = "";
    public string type { get; set; } = "";
    public bool site_admin { get; set; } = false;
    public string name { get; set; } = "";
    public string company { get; set; } = "";
    public string location { get; set; } = "";
    public string email { get; set; } = "";
    public string bio { get; set; } = "";
    public int public_repos { get; set; } = 0;
    public int total_private_repos { get; set; } = 0;
    public int owned_private_repos { get; set; } = 0;
    public bool two_factor_authentication { get; set; } = false;
    public DateTime? created_at { get; set; } = null;
    public DateTime? updated_at { get; set; } = null;
}

internal class GithubUserEmail
{
    public string email { get; set; } = "";
    public bool primary { get; set; } = false;
    public bool verified { get; set; } = false;
    public string visibility { get; set; } = "";
}

public class GitHubOAuthController: BaseOAuthController
{

    public GitHubOAuthController(
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

    public async Task<IResult> GetProviderLink_Github(HttpContext context)
    {
        try
        {
            var url = string.Empty;
            await Task.Run(() => {
                var clientId = _configuration.GetValue<string>("OAuth:GitHub:ClientId");
                var redirectUri = _configuration.GetValue<string>("OAuth:GitHub:RedirectUri");
                var scope = "user:email read:user repo read:org read:public_key read:gpg_key";
                var state = Helper.GenerateRandomString(10);
                url = $"https://github.com/login/oauth/authorize?client_id={clientId}&redirect_uri={redirectUri}&scope={scope}&state={state}&allow_signup=true";
                #pragma warning disable SYSLIB0013 // Type or member is obsolete
                url = Uri.EscapeUriString(url);
                #pragma warning restore SYSLIB0013 // Type or member is obsolete
            });
            return ResponseHandler.Ok("Github login link generated successfully!", new { Url = url });
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> ProviderRedirect_GitHub(HttpContext context, string code, string state)
    {
        try
        {
            var clientId = _configuration.GetValue<string>("OAuth:GitHub:ClientId");
            if (string.IsNullOrEmpty(clientId))
            {
                return ResponseHandler.InternalServerError("GitHub client id not found.");
            }
            var clientSecret = _configuration.GetValue<string>("OAuth:GitHub:ClientSecret");
            if (string.IsNullOrEmpty(clientSecret))
            {
                return ResponseHandler.InternalServerError("GitHub client secret not found.");
            }
            var url = $"https://github.com/login/oauth/access_token?client_id={clientId}&client_secret={clientSecret}&code={code}";
            //TODO: Add logic to compare state

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var response = await client.PostAsync(url, new StringContent(""));
            if (!response.IsSuccessStatusCode)
            {
                return ResponseHandler.InternalServerError("Unable to retrieve GitHub access token.");
            }
            var responseText = await response.Content.ReadAsStringAsync();
            if (responseText == null)
            {
                return ResponseHandler.InternalServerError("Unable to parse GitHub authozation response.");
            }
            // Deserialize json
            var responseModel = JsonSerializer.Deserialize<GithubAccessTokenModel>(responseText);
            if (responseModel == null)
            {
                return ResponseHandler.InternalServerError("Unable to deserialize GitHub response.");
            }
            var githubAccessToken = responseModel.access_token;
            var githubUser = await GetGithubUser(githubAccessToken);
            if (githubUser == null)
            {
                return ResponseHandler.InternalServerError("Unable to retrieve GitHub user's information.");
            }
            var email = githubUser.email;
            var name = githubUser.name ?? githubUser.login;
            var firstName = name;
            var lastName = string.Empty;
            if (!string.IsNullOrEmpty(name))
            {
                var names = name.Split(' ');
                if (names.Length > 1)
                {
                    firstName = names[0];
                    lastName = string.Join(' ', names[1..]);
                }
            }
            var (loginResponse, mfaChallengeResponse) = await signupOrLoginAsync(context, email, firstName, lastName, "GitHub");
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

    #region Privates

    private async Task<GithubUser?> GetGithubUser(string token)
    {
        try
        {
            string apiUrl = "https://api.github.com/user";
            string accessToken = token;
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            request.Headers.Add("User-Agent", "deft-dev/1.0");
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogError("Unable to retrieve GitHub access token.");
                _logger.LogError(content);
                return null;
            }
            _logger.LogInformation("GitHub user information retrieved successfully.");
            var responseText = await response.Content.ReadAsStringAsync();
            if (responseText == null)
            {
                _logger.LogError("Unable to parse GitHub authozation response.");
                return null;
            }
            var user = JsonSerializer.Deserialize<GithubUser>(responseText);
            if (user == null)
            {
                _logger.LogError("Unable to deserialize GitHub response.");
                return null;
            }
            if (string.IsNullOrEmpty(user.email))
            {
                var emailRquest = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
                emailRquest.Headers.Add("Authorization", $"Bearer {accessToken}");
                emailRquest.Headers.Add("User-Agent", "deft-dev/1.0");
                var emailResponse = await client.SendAsync(emailRquest);
                if (!emailResponse.IsSuccessStatusCode)
                {
                    var content = await emailResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Unable to retrieve GitHub user's email.");
                    _logger.LogError(content);
                    return user;
                }
                var emailResponseText = await emailResponse.Content.ReadAsStringAsync();
                if (emailResponseText == null)
                {
                    _logger.LogError("Unable to parse GitHub user's email response.");
                    return user;
                }
                var emails = JsonSerializer.Deserialize<List<GithubUserEmail>>(emailResponseText);
                if (emails == null || emails.Count == 0)
                {
                    _logger.LogError("GitHub user's email not found.");
                    return user;
                }
                var primaryEmail = string.Empty;
                for (int i = 0; i < emails.Count; i++)
                {
                    var e = emails[i];
                    var email = e.email;
                    if (e.primary)
                    {
                        primaryEmail = email;
                        break;
                    }
                    else if (!string.IsNullOrEmpty(email) && email.Contains("users.noreply.github.com"))
                    {
                        continue;
                    }
                }
                if (string.IsNullOrEmpty(primaryEmail))
                {
                    primaryEmail = emails.FirstOrDefault()?.email;
                }
                user.email = primaryEmail ?? user.email;
            }
            return user;
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
        }
        return null;
    }

    #endregion
}
