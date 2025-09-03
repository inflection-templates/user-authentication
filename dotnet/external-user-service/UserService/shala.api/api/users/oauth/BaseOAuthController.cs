using System.IdentityModel.Tokens.Jwt;
using shala.api.common;
using shala.api.domain.types;
using shala.api.services;
using shala.api.startup;

namespace shala.api;

public class BaseOAuthController
{
    protected readonly IConfiguration _configuration;
    protected readonly ILogger<UserController> _logger;
    protected readonly IUserService _userService;
    protected readonly IUserRoleService _userRoleService;
    protected readonly IUserAuthService _userAuthService;
    protected readonly IUserAuthProfileService _userAuthProfileService;
    protected readonly IRoleService _roleService;

    public BaseOAuthController(
                        IConfiguration configuration,
                        ILogger<UserController> logger,
                        IUserService UserService,
                        IRoleService roleService,
                        IUserRoleService userRoleService,
                        IUserAuthService userAuthService,
                        IUserAuthProfileService userAuthProfileService)
    {
        _configuration = configuration;
        _logger = logger;
        _userService = UserService;
        _roleService = roleService;
        _userAuthService = userAuthService;
        _userRoleService = userRoleService;
        _userAuthProfileService = userAuthProfileService;
    }

    public async Task<IResult> GetProviders(HttpContext context)
    {
        try
        {
            var providers = new List<string>();
            await Task.Run(() => {
                providers = new List<string>() {
                    "Google", "GitHub", "GitLab"
                };
            });
            return ResponseHandler.Ok("Providers fetched successfully", providers);
        }
        catch (Exception ex)
        {
            return ResponseHandler.ControllerException(ex);
        }
    }

    protected async Task<(LoginResponse?, MfaChallengeResponse?)> signupOrLoginAsync(
        HttpContext context,
        string email,
        string? firstName,
        string? lastName,
        string? provider = null)
    {
        var user = await _userService.GetByEmailAsync(email);
        if (user == null)
        {
            _logger.LogInformation("User not found, creating new user");
            var userName = await Helper.GetUniqueUsername(this._userService, firstName, lastName);
            var newUserModel = new UserCreateModel
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                UserName = userName,
            };
            var newUserRecord = await _userService.CreateAsync(newUserModel);
            if (newUserModel == null)
            {
                var message = "User could not be created";
                _logger.LogError(message);
                throw new Exception(message);
            }
            var userId = newUserRecord?.Id;
            if (userId == null)
            {
                throw new Exception("Invalid user record");
            }
            var userAuthProfile = await _userAuthProfileService.CreateUserAuthProfileAsync(userId ?? Guid.Empty, null);
            user = newUserRecord;
        }
        else
        {
            _logger.LogInformation("User found...");
        }
        if (user == null)
        {
            var message = "User could not be found or created";
            _logger.LogError(message);
            throw new Exception(message);
        }

        var mfaEnabled = await _userAuthProfileService.GetMfaEnabledAsync(user.Id);
        var preferredMfaType = await _userAuthProfileService.GetPreferredMfaTypeAsync(user.Id);
        var sessionModel = new SessionCreateModel
        {
            UserId = user.Id,
            OAuthProvider = provider,
            AuthenticationMethod = "OAuth",
            MfaEnabled = mfaEnabled,
            MfaType = preferredMfaType,
            UserAgent = context.Request.Headers["User-Agent"],
            IpAddress = context.Connection.RemoteIpAddress?.ToString() ?? "",
            StartedAt = DateTime.UtcNow,
        };
        var session = await _userAuthService.CreateSessionAsync(sessionModel);
        if (session == null)
        {
            var message = "Session cannot be created";
            _logger.LogError(message);
            throw new Exception(message);
        }

        if (mfaEnabled) {
            var challenge = new MfaChallengeResponse {
                UserId = user.Id,
                SessionId = session.Id,
                MfaEnabled = true,
                MfaType = preferredMfaType,
            };
            return (null, challenge);
        }

        var existingUserRoles = await _userRoleService.GetRolesForUserAsync(user.Id);
        Role? role = null;
        if (existingUserRoles == null || existingUserRoles.Count() == 0)
        {
            var basicRole = await _roleService.GetByNameAsync(DefaultRoles.User.ToString());
            if (basicRole == null)
            {
                var message = "Default role not found";
                _logger.LogError(message);
                throw new Exception(message);
            }
            var roleAdded = await _userRoleService.AddRoleToUserAsync(user.Id, basicRole.Id);
            if (!roleAdded)
            {
                var message = "User role could not be created";
                _logger.LogError(message);
                throw new Exception(message);
            }
            role = basicRole;
        }
        var token = UserAuthenticationHandler.GenerateAuthToken(this._configuration, user, session.Id, role?.Name);
        var refreshToken = UserAuthenticationHandler.GenerateRefreshToken(this._configuration, user, role?.Name);
        var fullName = (user.FirstName ?? "") + " " + (user.LastName ?? "");
        fullName = fullName.Trim();

        var res = new LoginResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = new JwtSecurityTokenHandler().WriteToken(refreshToken),
            Expiration = token.ValidTo,
            UserId = user.Id,
            UserName = user.UserName,
            SessionId = session.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            FullName = fullName,
            RoleName = role?.Name,
            ValidTill = session.ValidTill,
        };
        return (res, null);
    }

}
