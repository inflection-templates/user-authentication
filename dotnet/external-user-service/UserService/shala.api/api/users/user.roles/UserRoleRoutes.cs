using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using shala.api.domain.types;

namespace shala.api;

public static class UserRoleRoutes
{
    static readonly string BaseContextName = "Users.UserRoles";

    public static void Map(WebApplication app)
    {

        var router = app.MapGroup("/api/v1/user-roles");

        router.MapPost("/{userId}/roles/{roleId}", async (
            [FromServices] UserRoleController controller,
            Guid userId,
            Guid roleId,
            HttpContext context) => {
            var result = await controller.AddTenantRoleForUser(context, userId, roleId);
            return result;
        })
            .Accepts<Guid>("application/json")
            .WithName($"{BaseContextName}.AddTenantRoleForUser")
            .WithOpenApi()
            .RequireAuthorization()
            .Produces<IResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        router.MapDelete("/{userId}/roles/{roleId}", async (
            [FromServices] UserRoleController controller,
            Guid userId,
            Guid roleId,
            HttpContext context) => {
            var result = await controller.RemoveTenantRoleForUser(context, userId, roleId);
            return result;
        })
            .Accepts<Guid>("application/json")
            .WithName($"{BaseContextName}.RemoveTenantRoleForUser")
            .WithOpenApi()
            .RequireAuthorization()
            .Produces<IResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        router.MapGet("/{userId}", async (
            [FromServices] UserRoleController controller,
            Guid userId,
            HttpContext context) => {
            var result = await controller.GetRolesForUser(context, userId);
            return result;
        })
            .Accepts<Guid>("application/json")
            .WithName($"{BaseContextName}.GetRolesForUser")
            .WithOpenApi()
            .RequireAuthorization()
            .Produces<IResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

    }

}
