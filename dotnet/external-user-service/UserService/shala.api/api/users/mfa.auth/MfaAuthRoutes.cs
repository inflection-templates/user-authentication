using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using shala.api.domain.types;

namespace shala.api;

public static class MfaAuthRoutes
{
    static readonly string BaseContextName = "UserAuth:MFA";

    public static void Map(WebApplication app)
    {
        var router = app.MapGroup("/api/v1/auth/mfa");

        router.MapPost("/enable", async (
            [FromServices] MfaAuthController controller,
            HttpContext context) => {
            var result = await controller.EnableMfa(context);
            return result;
        })
        .WithName($"{BaseContextName}.EnableMfa")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        router.MapGet("/generate-totp-qrcode", async (
            [FromServices] MfaAuthController controller,
            HttpContext context) => {
            var result = await controller.GenerateTotpQRCode(context);
            return result;
        })
        .WithName($"{BaseContextName}.GenerateTotpQRCode")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        router.MapPost("/validate-totp", async (
            [FromServices] MfaAuthController controller,
            [FromBody] UserTotpValidationModel model,
            HttpContext context) => {
            var result = await controller.ValidateTotp(context, model);
            return result;
        })
        .Accepts<UserTotpValidationModel>("application/json")
        .WithName($"{BaseContextName}.ValidateTotp")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

    }

}


