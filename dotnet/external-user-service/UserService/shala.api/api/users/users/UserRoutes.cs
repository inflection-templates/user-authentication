using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using shala.api.domain.types;
using shala.api.startup;

namespace shala.api;

public static class UserRoutes
{
    static readonly string BaseContextName = "Users";

    public static void Map(WebApplication app)
    {

        var router = app.MapGroup("/api/v1/users");

        router.MapPost("", async (
            [FromServices] UserController controller,
            [FromBody] UserCreateModel model,
            HttpContext context) => {
            var result = await controller.Create(context, model);
            return result;
        })
        .Accepts<UserCreateModel>("application/json")
        .WithName($"{BaseContextName}.Create")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status409Conflict);

        router.MapGet("/search", async (
            [FromServices] UserController controller,
            HttpContext context,
            [FromQuery] string? firstName = null,
            [FromQuery] string? lastName = null,
            [FromQuery] string? email = null,
            [FromQuery] string? countryCode = null,
            [FromQuery] string? phoneNumber = null,
            [FromQuery] Guid? tenantId = null,
            [FromQuery] int? pageIndex = 0,
            [FromQuery] int? itemsPerPage = 10,
            [FromQuery] string? orderBy = null,
            [FromQuery] string? order = null
            ) => {

            var sortOrder = order == SortOrder.Ascending.ToString() ? SortOrder.Ascending : SortOrder.Descending;
            var filters = new UserSearchFilters
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                CountryCode = countryCode,
                PhoneNumber = phoneNumber,
                TenantId = tenantId,
                PageIndex = pageIndex ?? 0,
                ItemsPerPage = itemsPerPage ?? 10,
                OrderBy = orderBy,
                Order = sortOrder,
            };
            var result = await controller.Search(context, filters);
            return result;
        })
        .Accepts<UserSearchFilters>("application/json")
        .WithName($"{BaseContextName}.Search")
        .WithOpenApi()
        .RequireAuthorization()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        router.MapGet("/{id}", async (
            [FromServices] UserController controller,
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

        router.MapPut("/{id}", async (
            [FromServices] UserController controller,
            Guid id,
            [FromBody] UserUpdateModel model,
            HttpContext context) => {
            var result = await controller.Update(context, id, model);
            return result;
        })
        .Accepts<UserUpdateModel>("application/json")
        .WithName($"{BaseContextName}.Update")
        .WithOpenApi()
        .RequireAuthorization()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

        router.MapDelete("/{id}", async (
            [FromServices] UserController controller,
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

        // Profile endpoint for getting current user profile
        router.MapGet("profile", async (
            [FromServices] UserController controller,
            HttpContext context) => {
            // Extract user ID from JWT token or authentication context
            var currentUser = UserAuthenticationHandler.GetCurrentUser(context);
            if (currentUser == null)
            {
                return Results.Unauthorized();
            }
            
            var result = await controller.GetById(context, currentUser.UserId);
            return result;
        })
        .WithName($"{BaseContextName}.GetProfile")
        .WithOpenApi()
        .RequireAuthorization()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status404NotFound);

    }

}
