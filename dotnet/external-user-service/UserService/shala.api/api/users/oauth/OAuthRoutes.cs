using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace shala.api;

public static class OAuthRoutes
{
    static readonly string BaseContextName = "Users.OAuth";

    public static void Map(WebApplication app)
    {

        var router = app.MapGroup("/api/v1/oauth");

        router.MapGet("/providers", async (
            [FromServices] BaseOAuthController controller,
            HttpContext context) => {
            return await controller.GetProviders(context);
        })
        .WithName($"{BaseContextName}.Providers")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        router.MapGet("/common/challenge/{provider}", async (
            [FromServices] BaseOAuthController controller,
            [FromRoute] string provider,
            HttpContext context) => {
            // var redirectUrl = $"{context.Request.Scheme}://{context.Request.Host}/api/v1/oauth/google/login";
            var redirectUrl = $"/api/v1/oauth/google/callback";
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            await context.ChallengeAsync("google", properties);
        })
        .WithName($"{BaseContextName}.Challenge.Common")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        router.MapGet("/google/challenge", async (
            [FromServices] GoogleOAuthController controller,
            HttpContext context) => {
            return await controller.GetProviderLink_Google(context);
        })
        .WithName($"{BaseContextName}.ProviderLink.Google")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        router.MapGet("/google/callback", async (
            [FromServices] GoogleOAuthController controller,
            [FromQuery] string code,
            HttpContext context) => {
            var result = await controller.Login(context, "google", code, string.Empty);
            return result;
        })
        .WithName($"{BaseContextName}.Callback.Google")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        router.MapGet("/github/challenge", async (
            [FromServices] GitHubOAuthController controller,
            HttpContext context) => {
            return await controller.GetProviderLink_Github(context);
        })
        .WithName($"{BaseContextName}.ProviderLink.GitHub")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        router.MapGet("/github/callback", async (
            [FromServices] GitHubOAuthController controller,
            [FromQuery] string code,
            [FromQuery] string state,
            HttpContext context) => {
            var result = await controller.ProviderRedirect_GitHub(context, code, state);
            return result;
        })
        .WithName($"{BaseContextName}.Callback.GitHub")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        router.MapGet("/gitlab/challenge", async (
            [FromServices] GitLabOAuthController controller,
            HttpContext context) => {
            return await controller.GetProviderLink_Gitlab(context);
        })
        .WithName($"{BaseContextName}.ProviderLink.GitLab")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        router.MapGet("/gitlab/callback", async (
            [FromServices] GitLabOAuthController controller,
            [FromQuery] string code,
            [FromQuery] string state,
            HttpContext context) => {
            var result = await controller.ProviderRedirect_GitLab(context, code, state);
            return result;
        })
        .WithName($"{BaseContextName}.Callback.GitLab")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        router.MapGet("/facebook/challenge", async (
            [FromServices] FacebookOAuthController controller,
            HttpContext context) => {
            return await controller.GetProviderLink_Facebook(context);
        })
        .WithName($"{BaseContextName}.ProviderLink.Facebook")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        router.MapGet("/facebook/callback", async (
            [FromServices] FacebookOAuthController controller,
            [FromQuery] string code,
            [FromQuery] string state,
            HttpContext context) => {
            var result = await controller.Login(context, "Facebook", code, state);
            return result;
        })
        .WithName($"{BaseContextName}.Callback.Facebook")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        router.MapGet("/twitter/challenge", async (
            [FromServices] TwitterOAuthController controller,
            HttpContext context) => {
            return await controller.GetProviderLink_Twitter(context);
        })
        .WithName($"{BaseContextName}.ProviderLink.Twitter")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        router.MapGet("/twitter/callback", async (
            [FromServices] TwitterOAuthController controller,
            [FromQuery] string code,
            [FromQuery] string state,
            HttpContext context) => {
            var result = await controller.Login(context, "Twitter", code, state);
            return result;
        })
        .WithName($"{BaseContextName}.Callback.Twitter")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

    }

}
