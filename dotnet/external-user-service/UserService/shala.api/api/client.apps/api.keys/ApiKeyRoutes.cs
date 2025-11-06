using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using shala.api.domain.types;

namespace shala.api;

public static class ApiKeyRoutes
{
    static readonly string BaseContextName = "ClientApps.ApiKeys";

    public static void Map(WebApplication app)
    {
        var router = app.MapGroup("/api/v1/api-keys");

        router.MapPost("", async (
            [FromServices] ApiKeyController controller,
            [FromBody] ApiKeyCreateRequestModel model,
            HttpContext context) =>
        {
            var result = await controller.GenerateApiKey(context, model);
            return result;
        })
        .Accepts<ApiKeyCreateRequestModel>("application/json")
        .WithName($"{BaseContextName}.CreateApiKey")
        .WithOpenApi()
        .RequireAuthorization()
        .Produces<IResult>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        router.MapGet("/search", async (
            [FromServices] ApiKeyController controller,
            HttpContext context,
            [FromQuery] Guid? clientAppId = null,
            [FromQuery] string? name = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int? pageIndex = 0,
            [FromQuery] int? itemsPerPage = 10,
            [FromQuery] string? orderBy = null,
            [FromQuery] string? order = null
            ) => {
            var sortOrder = order == SortOrder.Ascending.ToString() ? SortOrder.Ascending : SortOrder.Descending;
            var filters = new ApiKeySearchFilters
            {
                ClientAppId = clientAppId,
                Name = name,
                IsActive = isActive,
                PageIndex = pageIndex ?? 0,
                ItemsPerPage = itemsPerPage ?? 10,
                OrderBy = orderBy,
                Order = sortOrder,
            };
            var result = await controller.SearchApiKeys(context, filters);
            return result;
        })
        .Accepts<ApiKeySearchFilters>("application/json")
        .WithName($"{BaseContextName}.SearchApiKeys")
        .WithOpenApi()
        .RequireAuthorization()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        router.MapDelete("/{id}", async (
            [FromServices] ApiKeyController controller,
            Guid apiKeyId,
            HttpContext context) =>
        {
            var result = await controller.DeleteApiKey(context, apiKeyId);
            return result;
        })
        .Accepts<Guid>("application/json")
        .WithName($"{BaseContextName}.DeleteApiKey")
        .WithOpenApi()
        .RequireAuthorization()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        router.MapGet("/client-apps/{clientAppId}", async (
            [FromServices] ApiKeyController controller,
            Guid clientAppId,
            HttpContext context) =>
        {
            var result = await controller.GetByClientAppIdAsync(context, clientAppId);
            return result;
        })
        .Accepts<Guid>("application/json")
        .WithName($"{BaseContextName}.GetApiKeysForClientApp")
        .WithOpenApi()
        .RequireAuthorization()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        router.MapDelete("/client-apps/{clientAppId}", async (
            [FromServices] ApiKeyController controller,
            Guid id,
            HttpContext context) =>
        {
            var result = await controller.DeleteByClientAppId(context, id);
            return result;
        })
        .Accepts<Guid>("application/json")
        .WithName($"{BaseContextName}.DeleteByClientAppId")
        .WithOpenApi()
        .RequireAuthorization()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

    }

}
