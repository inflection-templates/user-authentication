using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using shala.api.domain.types;

namespace shala.api;

public static class TenantRoutes
{
    static readonly string BaseContextName = "Tenants";

    public static void Map(WebApplication app)
    {
        var router = app.MapGroup("/api/v1/tenants");

        router.MapPost("", async (
            [FromServices] TenantController controller,
            [FromBody] TenantCreateModel model,
            HttpContext context) => {
            var result = await controller.Create(context, model);
            return result;
        })
        .Accepts<TenantCreateModel>("application/json")
        .WithName($"{BaseContextName}.Create")
        .WithOpenApi()
        .RequireAuthorization()
        .Produces<IResult>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status409Conflict);

        router.MapGet("/search", async (
            [FromServices] TenantController controller,
            HttpContext context,
            [FromQuery] string? name = null,
            [FromQuery] string? code = null,
            [FromQuery] string? phoneNumber = null,
            [FromQuery] string? email = null,
            [FromQuery] int? pageIndex = 0,
            [FromQuery] int? itemsPerPage = 10,
            [FromQuery] string? orderBy = null,
            [FromQuery] string? order = null
            ) => {
            var sortOrder = order == SortOrder.Ascending.ToString() ? SortOrder.Ascending : SortOrder.Descending;
            var filters = new TenantSearchFilters
            {
                Name = name,
                Code = code,
                PhoneNumber = phoneNumber,
                Email = email,
                PageIndex = pageIndex ?? 0,
                ItemsPerPage = itemsPerPage ?? 10,
                OrderBy = orderBy,
                Order = sortOrder,
            };
            var result = await controller.Search(context, filters);
            return result;
        })
        .Accepts<TenantSearchFilters>("application/json")
        .WithName($"{BaseContextName}.Search")
        .WithOpenApi()
        .RequireAuthorization()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        router.MapGet("/{id}", async (
            [FromServices] TenantController controller,
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
            [FromServices] TenantController controller,
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
            [FromServices] TenantController controller,
            Guid id,
            [FromBody] TenantUpdateModel model,
            HttpContext context) => {
            var result = await controller.Update(context, id, model);
            return result;
        })
        .Accepts<TenantUpdateModel>("application/json")
        .WithName($"{BaseContextName}.Update")
        .WithOpenApi()
        .RequireAuthorization()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        router.MapDelete("/{id}", async (
            [FromServices] TenantController controller,
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
