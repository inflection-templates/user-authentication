using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using shala.api.domain.types;

namespace shala.api;

public static class UserAuthRoutes
{
    static readonly string BaseContextName = "UserAuth";

    public static void Map(WebApplication app)
    {
        var router = app.MapGroup("/api/v1/auth");

        router.MapPost("/login-with-password", async (
            [FromServices] UserAuthController controller,
            [FromBody] UserPasswordLoginModel model,
            HttpContext context) => {
            var result = await controller.LoginWithPassword(context, model);
            return result;
        })
        .Accepts<UserPasswordLoginModel>("application/json")
        .WithName($"{BaseContextName}.LoginWithPassword")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        router.MapPost("/send-otp", async (
            [FromServices] UserAuthController controller,
            [FromBody] UserSendOtpModel model,
            HttpContext context) => {
            var result = await controller.SendOtp(context, model);
            return result;
        })
        .Accepts<UserSendOtpModel>("application/json")
        .WithName($"{BaseContextName}.SendOtp")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        router.MapPost("/login-with-otp", async (
            [FromServices] UserAuthController controller,
            [FromBody] UserOtpLoginModel model,
            HttpContext context) => {
            var result = await controller.LoginWithOtp(context, model);
            return result;
        })
        .Accepts<UserOtpLoginModel>("application/json")
        .WithName($"{BaseContextName}.LoginWithOtp")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        router.MapPost("/reset-password/send-link", async (
            [FromServices] UserAuthController controller,
            [FromBody] UserResetPasswordSendLinkModel model,
            HttpContext context) => {
            var result = await controller.ResetPasswordSendLink(context, model);
            return result;
        })
        .Accepts<UserResetPasswordSendLinkModel>("application/json")
        .WithName($"{BaseContextName}.ResetPasswordSendLink")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        router.MapPost("/reset-password", async (
            [FromServices] UserAuthController controller,
            [FromBody] UserResetPasswordModel model,
            HttpContext context) => {
            var result = await controller.ResetPassword(context, model);
            return result;
        })
        .Accepts<UserResetPasswordModel>("application/json")
        .WithName($"{BaseContextName}.ResetPassword")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        router.MapPost("/change-password", async (
            [FromServices] UserAuthController controller,
            [FromBody] UserChangePasswordModel model,
            HttpContext context) => {
            var result = await controller.ChangePassword(context, model);
            return result;
        })
        .Accepts<UserChangePasswordModel>("application/json")
        .WithName($"{BaseContextName}.ChangePassword")
        .WithOpenApi()
        .RequireAuthorization()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        router.MapPost("/rotate-token", async (
            [FromServices] UserAuthController controller,
            [FromBody] UserRefreshTokenModel refreshToken,
            HttpContext context) => {
            var result = await controller.RefreshToken(context, refreshToken);
            return result;
        })
        .Accepts<string>("application/json")
        .WithName($"{BaseContextName}.RefreshToken")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        router.MapPost("/logout", async (
            [FromServices] UserAuthController controller,
            HttpContext context) => {
            var result = await controller.Logout(context);
            return result;
        })
        .WithName($"{BaseContextName}.Logout")
        .WithOpenApi()
        .RequireAuthorization()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

    }
}
