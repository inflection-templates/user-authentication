using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using shala.api.common;
using shala.api.services;

namespace shala.api;

public class CommonOAuthController: BaseOAuthController
{
    public CommonOAuthController(
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

    public async Task<IResult> Login(HttpContext context, string provider)
    {
        try
        {
            var authenticateResult_ = await context.AuthenticateAsync(provider);
            var authenticateResult = await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded)
            {
                return Results.Unauthorized();
            }
            var oauthUser = authenticateResult.Principal;
            if (oauthUser == null)
            {
                return Results.Unauthorized();
            }
            var email = oauthUser.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return Results.Unauthorized();
            }
            var name = oauthUser.Identity?.Name;
            var lastNameClaim = oauthUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname) ?? oauthUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            var givenNameClaim = oauthUser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName);
            var firstName = givenNameClaim?.Value ?? name;
            var lastName = lastNameClaim?.Value;

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

