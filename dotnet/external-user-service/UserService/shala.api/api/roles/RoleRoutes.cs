using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using shala.api.domain.types;

namespace shala.api;

public static class RoleRoutes
{
    static readonly string BaseContextName = "Roles";

    public static void Map(WebApplication app)
    {
        var router = app.MapGroup("/api/v1/roles");

        router.MapPost("", async (
            [FromServices] RoleController controller,
            [FromBody] RoleCreateModel model,
            HttpContext context) => {
                var result = await controller.Create(context, model);
                return result;
            })
            .Accepts<RoleCreateModel>("application/json")
            .WithName($"{BaseContextName}.Create")
            .WithOpenApi()
            .RequireAuthorization()
            .Produces<IResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        router.MapGet("/{id}", async (
            [FromServices] RoleController controller,
            Guid id,
            HttpContext context) =>
            {
                var result = await controller.GetById(context, id
                );
                return result;
            })
            .Accepts<Guid>("application/json")
            .WithName($"{BaseContextName}.GetById")
            .WithOpenApi()
            .RequireAuthorization()
            .Produces<IResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        router.MapGet("/by-name/{name}", async (
            [FromServices] RoleController controller,
            string name,
            HttpContext context) =>
            {
                var result = await controller.GetByName(context, name);
                return result;
            })
            .Accepts<string>("application/json")
            .WithName($"{BaseContextName}.GetByName")
            .WithOpenApi()
            .RequireAuthorization()
            .Produces<IResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        router.MapPut("/{id}", async (
            [FromServices] RoleController controller,
            Guid id,
            [FromBody] RoleUpdateModel model,
            HttpContext context) => {
            var result = await controller.Update(context, id, model);
            return result;
            })
            .Accepts<Guid>("application/json")
            .WithName($"{BaseContextName}.Update")
            .WithOpenApi()
            .RequireAuthorization()
            .Produces<IResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

        router.MapGet("/search", async (
            [FromServices] RoleController controller,
            HttpContext context,
            [FromQuery] string? name = null,
            [FromQuery] Guid? tenantId = null,
            [FromQuery] int? pageIndex = 0,
            [FromQuery] int? itemsPerPage = 25,
            [FromQuery] string? orderBy = null,
            [FromQuery] string? order = null
            ) => {

            var sortOrder = order == SortOrder.Ascending.ToString() ? SortOrder.Ascending : SortOrder.Descending;
            var filters = new RoleSearchFilters
            {
                Name = name,
                TenantId = tenantId,
                PageIndex = pageIndex ?? 0,
                ItemsPerPage = itemsPerPage ?? 25,
                OrderBy = orderBy,
                Order = sortOrder,
            };
            var result = await controller.Search(context, filters);
            return result;
            })
            .Accepts<RoleSearchFilters>("application/json")
            .WithName($"{BaseContextName}.Search")
            .WithOpenApi()
            .RequireAuthorization()
            .Produces<IResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        router.MapDelete("/{id}", async (
            [FromServices] RoleController controller,
            Guid id,
            HttpContext context) => {
            var result = await controller.Delete(context, id);
            return result;
            })
            .Accepts<Guid>("application/json")
            .WithName($"{BaseContextName}.Delete")
            .WithOpenApi()
            .RequireAuthorization()
            .Produces<IResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);

    }
}
