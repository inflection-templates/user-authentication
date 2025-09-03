using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using shala.api.domain.types;
using shala.api.startup;

namespace shala.api;

public static class FileResourceRoutes
{
    static readonly string BaseContextName = "FileResource";

    public static void Map(WebApplication app)
    {
        var router = app.MapGroup("/api/v1/file-resources").DisableAntiforgery();;

        router.MapPost("/upload", async (
            [FromServices] FileResourceController controller,
            HttpContext context,
            IFormFile file,
            [FromQuery] bool? isPublic = false) =>{
            var result = await controller.Upload(context, file, isPublic ?? false);
            return result;
        })
        .Accepts<IFormFile>("multipart/form-data")
        .WithName($"{BaseContextName}.Upload")
        .RequireAuthorization()
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);

        router.MapPost("/upload-many", async (
            [FromServices] FileResourceController controller,
            HttpContext context,
            IEnumerable<IFormFile> files,
            [FromQuery] bool? isPublic = false) =>{
            var result = await controller.UploadMany(context, files, isPublic ?? false);
            return result;
        })
        .Accepts<IEnumerable<IFormFile>>("multipart/form-data")
        .WithName($"{BaseContextName}.UploadMany")
        .RequireAuthorization()
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);


        router.MapGet("/download/{id}", async (
            [FromServices] FileResourceController controller,
            Guid id,
            HttpContext context) => {
            var result = await controller.Download(context, id);
            return result;
        })
        .Accepts<Guid>("application/json")
        .WithName($"{BaseContextName}.Download")
        .RequireAuthorization()
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        router.MapGet("/download-public/{id}", async (
            [FromServices] FileResourceController controller,
            Guid id,
            HttpContext context) => {
            var result = await controller.DownloadPublic(context, id);
            return result;
        })
        .Accepts<Guid>("application/json")
        .WithName($"{BaseContextName}.DownloadPublic")
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        router.MapGet("/search", async (
            [FromServices] FileResourceController controller,
            HttpContext context,
            [FromQuery] Guid? userId = null,
            [FromQuery] Guid? tenantId = null,
            [FromQuery] int? pageIndex = 0,
            [FromQuery] int? itemsPerPage = 10,
            [FromQuery] string? orderBy = null,
            [FromQuery] string? order = null
            ) => {

            var sortOrder = order == SortOrder.Ascending.ToString() ? SortOrder.Ascending : SortOrder.Descending;
            var currentUser = UserAuthenticationHandler.GetCurrentUser(context);
            var filters = new FileResourceSearchFilters
            {
                OwnerUserId = userId ?? currentUser.UserId,
                TenantId = tenantId ?? currentUser.TenantId,
                PageIndex = pageIndex ?? 0,
                ItemsPerPage = itemsPerPage ?? 10,
                OrderBy = orderBy,
                Order = sortOrder,
            };
            var result = await controller.Search(context, filters);
            return result;
        })
        .Accepts<FileResourceSearchFilters>("application/json")
        .WithName($"{BaseContextName}.Search")
        .RequireAuthorization()
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        router.MapGet("/{id}", async (
            [FromServices] FileResourceController controller,
            Guid id,
            HttpContext context) => {
            var result = await controller.GetById(context, id);
            return result;
        })
        .Accepts<Guid>("application/json")
        .WithName($"{BaseContextName}.GetById")
        .RequireAuthorization()
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        router.MapDelete("/{id}", async (
            [FromServices] FileResourceController controller,
            Guid id,
            HttpContext context) => {
            var result = await controller.Delete(context, id);
            return result;
        })
        .Accepts<Guid>("application/json")
        .WithName($"{BaseContextName}.Delete")
        .RequireAuthorization()
        .WithOpenApi()
        .Produces<IResult>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

    }
}

