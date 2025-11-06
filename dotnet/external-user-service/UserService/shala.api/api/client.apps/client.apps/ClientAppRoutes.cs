using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using shala.api.domain.types;

namespace shala.api;

public static class ClientAppRoutes
{
    static readonly string BaseContextName = "ClientApps";

    public static void Map(WebApplication app)
    {
        var router = app.MapGroup("/api/v1/client-apps");

        router.MapPost("", async (
            [FromServices] ClientAppController controller,
            [FromBody] ClientAppCreateModel model,
            HttpContext context) => {
            var result = await controller.Create(context, model);
            return result;
        })
        .Accepts<ClientAppCreateModel>("application/json")
        .WithName($"{BaseContextName}.Create")
        .WithOpenApi()
        .RequireAuthorization()
        .Produces<IResult>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status409Conflict);

        router.MapGet("/search", async (
            [FromServices] ClientAppController controller,
            HttpContext context,
            [FromQuery] Guid? ownerUserId = null,
            [FromQuery] string? name = null,
            [FromQuery] string? code = null,
            [FromQuery] int? pageIndex = 0,
            [FromQuery] int? itemsPerPage = 10,
            [FromQuery] string? orderBy = null,
            [FromQuery] string? order = null
            ) => {
            var sortOrder = order == SortOrder.Ascending.ToString() ? SortOrder.Ascending : SortOrder.Descending;
            var filters = new ClientAppSearchFilters
            {
                OwnerUserId = ownerUserId,
                Name = name,
                Code = code,
                PageIndex = pageIndex ?? 0,
                ItemsPerPage = itemsPerPage ?? 10,
                OrderBy = orderBy,
                Order = sortOrder,
            };
            var result = await controller.Search(context, filters);
            return result;
        })
        .Accepts<ClientAppSearchFilters>("application/json")
        .WithName($"{BaseContextName}.Search")
        .WithOpenApi()
        .RequireAuthorization()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        router.MapGet("/{id}", async (
            [FromServices] ClientAppController controller,
            Guid id,
            HttpContext context) => {
            var result = await controller.GetById(context, id);
            return result;
        })
        .Accepts<Guid>("application/json")
        .WithName($"{BaseContextName}.GetById")
        .WithOpenApi()
        .RequireAuthorization()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        router.MapGet("/by-code/{code}", async (
            [FromServices] ClientAppController controller,
            string code,
            HttpContext context) => {
            var result = await controller.GetByCode(context, code);
            return result;
        })
        .Accepts<string>("application/json")
        .WithName($"{BaseContextName}.GetByCode")
        .WithOpenApi()
        .RequireAuthorization()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        router.MapPut("/{id}", async (
            [FromServices] ClientAppController controller,
            Guid id,
            [FromBody] ClientAppUpdateModel model,
            HttpContext context) => {
            var result = await controller.Update(context, id, model);
            return result;
        })
        .Accepts<ClientAppUpdateModel>("application/json")
        .WithName($"{BaseContextName}.Update")
        .WithOpenApi()
        .RequireAuthorization()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        router.MapDelete("/{id}", async (
            [FromServices] ClientAppController controller,
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
