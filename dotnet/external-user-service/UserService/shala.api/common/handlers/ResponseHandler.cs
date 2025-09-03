using Serilog;

namespace shala.api.common;

public class CustomResponse
{
    public string Status { get; set; } = "success";
    public string Message { get; set; } = "Request successfully processed";
    public object? Data { get; set; } = null;
    public string? Url { get; set; } = null;
}

public static class ResponseHandler
{

    public static IResult ValidationError(string message, Dictionary<string, string[]> errors)
    {
        // extract validation errors as list of strings
        var errorList = errors.SelectMany(e => e.Value).ToList();
        var responseObject = new CustomResponse { Status = "failure", Message = message, Data = errorList };
        return Results.Json(responseObject, statusCode: StatusCodes.Status422UnprocessableEntity);
    }

    public static IResult InternalServerError(string message)
    {
        var responseObject = new CustomResponse { Status = "failure", Message = message };
        return Results.Json(responseObject, statusCode: StatusCodes.Status500InternalServerError);
    }

    public static IResult BadRequest(string message)
    {
        var responseObject = new CustomResponse { Status = "failure", Message = message };
        return Results.Json(responseObject, statusCode: StatusCodes.Status400BadRequest);
    }

    public static IResult Created(string message, object? obj = null, string? url = null)
    {
        var responseObject = new CustomResponse {
            Status = "success",
            Message = message,
            Data = obj,
            Url = url
        };
        return Results.Json(responseObject, statusCode: StatusCodes.Status201Created);
    }

    public static IResult Ok(string message)
    {
        var responseObject = new { Status = "success", Message = message };
        var result = new CustomResponse()
        {
            Status = "success",
            Message = message
        };
        return Results.Json(responseObject, statusCode: StatusCodes.Status200OK);
    }

    public static IResult Ok(string message, object? data, string? url = null)
    {
        var responseObject = new CustomResponse {
            Status = "success",
            Message = message,
            Data = data,
            Url = url
        };
        return Results.Json(responseObject, statusCode: StatusCodes.Status200OK);
    }

    public static IResult NotFound(string message)
    {
        var responseObject = new CustomResponse{ Status = "failure", Message = message };
        return Results.Json(responseObject, statusCode: StatusCodes.Status404NotFound);
    }

    public static IResult NoContent(string message)
    {
        var responseObject = new CustomResponse { Status = "failure", Message = message };
        return Results.Json(responseObject, statusCode: StatusCodes.Status204NoContent);
    }

    public static IResult Conflict(string message)
    {
        var responseObject = new CustomResponse { Status = "failure", Message = message };
        return Results.Json(responseObject, statusCode: StatusCodes.Status409Conflict);
    }

    public static IResult Unauthorized(string message)
    {
        var responseObject = new CustomResponse { Status = "failure", Message = message };
        return Results.Json(responseObject, statusCode: StatusCodes.Status401Unauthorized);
    }

    public static IResult Forbidden(string message)
    {
        var responseObject = new CustomResponse { Status = "failure", Message = message };
        return Results.Json(responseObject, statusCode: StatusCodes.Status403Forbidden);
    }

    public static IResult Redirect(string url, Boolean permanent = false)
    {
        return Results.Redirect(url, permanent: permanent);
    }

    public static IResult ControllerException(Exception exception)
    {
        Log.Error(exception, exception.Message);

        var responseObject = new CustomResponse {
            Status = "failure",
            Message = exception.Message,
            Data = null
        };

        if (exception is InvalidApiKeyException)
        {
            responseObject = new CustomResponse { Status = "failure", Message = exception.Message };
            return Results.Json(responseObject, statusCode: StatusCodes.Status401Unauthorized);
        }
        if (exception is MissingApiKeyException)
        {
            responseObject = new CustomResponse { Status = "failure", Message = exception.Message };
            return Results.Json(responseObject, statusCode: StatusCodes.Status401Unauthorized);
        }
        if (exception is UnauthorizedUserException)
        {
            responseObject = new CustomResponse { Status = "failure", Message = exception.Message };
            return Results.Json(responseObject, statusCode: StatusCodes.Status401Unauthorized);
        }
        //var trace = exception.StackTrace; // Please log this stack-trace
        Log.Error(exception, "A controller exception has occurred");
        // Log.Debug("Trace: ", exception.StackTrace);

        return Results.Json(responseObject, statusCode: StatusCodes.Status500InternalServerError);
    }
}
