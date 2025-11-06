using System.Text.Json;
using shala.api.common;
using shala.api.services;

namespace shala.api;


internal class GitlabAccessTokenModel
{
    public string access_token { get; set; } = string.Empty;
    public string token_type { get; set; } = string.Empty;
    public int expires_in { get; set; } = 0;
    public string refresh_token { get; set; } = string.Empty;
    public string scope { get; set; } = string.Empty;
    public long create_at { get; set; } = 0;
    public string id_token { get; set; } = string.Empty;

}

internal class GitlabUser
{
    public int id { get; set; }
    public string? username { get; set; }
    public string? name { get; set; }
    public string? state { get; set; }
    public bool locked { get; set; }
    public string? avatar_url { get; set; }
    public string? web_url { get; set; }
    public DateTime created_at { get; set; }
    public string? bio { get; set; }
    public string? location { get; set; }
    public string? public_email { get; set; }
    public string? skype { get; set; }
    public string? linkedin { get; set; }
    public string? twitter { get; set; }
    public string? discord { get; set; }
    public string? website_url { get; set; }
    public string? organization { get; set; }
    public string? job_title { get; set; }
    public string? pronouns { get; set; }
    public bool bot { get; set; }
    public string? work_information { get; set; }
    public object? local_time { get; set; }
    public DateTime last_sign_in_at { get; set; }
    public DateTime confirmed_at { get; set; }
    public string? last_activity_on { get; set; }
    public string email { get; set; } = string.Empty;
    public int theme_id { get; set; }
    public int color_scheme_id { get; set; }
    public int projects_limit { get; set; }
    public DateTime current_sign_in_at { get; set; }
    public object[]? identities { get; set; }
    public bool can_create_group { get; set; }
    public bool can_create_project { get; set; }
    public bool two_factor_enabled { get; set; }
    public bool external { get; set; }
    public bool private_profile { get; set; }
    public string? commit_email { get; set; }
    public object? shared_runners_minutes_limit { get; set; }
    public object? extra_shared_runners_minutes_limit { get; set; }
    public object[]? scim_identities { get; set; }
}

public class GitLabOAuthController: BaseOAuthController
{

    public GitLabOAuthController(
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

    public async Task<IResult> GetProviderLink_Gitlab(HttpContext context)
    {
        try
        {
            var url = string.Empty;
            await Task.Run(() => {
                var clientId = _configuration.GetValue<string>("OAuth:GitLab:ClientId");
                var redirectUri = "http://localhost:5089/api/v1/oauth/gitlab/callback";
                var scope = "read_user email openid read_api api read_repository write_repository";
                var state = Helper.GenerateRandomString(10);
                url = $"https://gitlab.com/oauth/authorize?client_id={clientId}&redirect_uri={redirectUri}";
                url += $"&response_type=code";
                url += $"&scope={scope}";
                url += $"&state={state}";
                #pragma warning disable SYSLIB0013 // Type or member is obsolete
                url = Uri.EscapeUriString(url);
                #pragma warning restore SYSLIB0013 // Type or member is obsolete
            });
            return ResponseHandler.Ok("Gitlab login link generated successfully!", new { Url = url });
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    public async Task<IResult> ProviderRedirect_GitLab(HttpContext context, string code, string state)
    {
        try
        {
            var clientId = _configuration.GetValue<string>("OAuth:GitLab:ClientId");
            if (string.IsNullOrEmpty(clientId))
            {
                return ResponseHandler.InternalServerError("GitLab client id not found.");
            }
            var clientSecret = _configuration.GetValue<string>("OAuth:GitLab:ClientSecret");
            if (string.IsNullOrEmpty(clientSecret))
            {
                return ResponseHandler.InternalServerError("GitLab client secret not found.");
            }
            var url = $"https://gitlab.com/oauth/token?client_id={clientId}&client_secret={clientSecret}&code={code}&grant_type=authorization_code&redirect_uri=http://localhost:5089/api/v1/oauth/gitlab/callback";
            //TODO: Add logic to compare state

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            var response = await client.PostAsync(url, new StringContent(""));
            if (!response.IsSuccessStatusCode)
            {
                return ResponseHandler.InternalServerError("Unable to retrieve GitLab access token.");
            }
            var responseText = await response.Content.ReadAsStringAsync();
            if (responseText == null)
            {
                return ResponseHandler.InternalServerError("Unable to parse GitLab authozation response.");
            }
            // Deserialize json
            var responseModel = JsonSerializer.Deserialize<GitlabAccessTokenModel>(responseText);
            if (responseModel == null)
            {
                return ResponseHandler.InternalServerError("Unable to deserialize GitLab response.");
            }
            var gitlabAccessToken = responseModel.access_token;
            var gitlabUser = await GetGitlabUser(gitlabAccessToken);
            if (gitlabUser == null)
            {
                return ResponseHandler.InternalServerError("Unable to retrieve GitLab user's information.");
            }
            var email = gitlabUser.email;
            var name = gitlabUser.name ?? gitlabUser.username;
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
            var (loginResponse, mfaChallengeResponse) = await signupOrLoginAsync(context, email, firstName, lastName, "GitLab");
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

    private async Task<GitlabUser?> GetGitlabUser(string token)
    {
        try
        {
            string apiUrl = "https://gitlab.com/api/v4/user";
            string accessToken = token;
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            request.Headers.Add("User-Agent", "deft-dev/1.0");
            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogError("Unable to retrieve GitLab access token.");
                _logger.LogError(content);
                return null;
            }
            _logger.LogInformation("GitLab user information retrieved successfully.");
            var responseText = await response.Content.ReadAsStringAsync();
            if (responseText == null)
            {
                _logger.LogError("Unable to parse GitLab authozation response.");
                return null;
            }
            var user = JsonSerializer.Deserialize<GitlabUser>(responseText);
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
