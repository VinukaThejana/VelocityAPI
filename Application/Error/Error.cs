using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace VelocityAPI.Application.Error;

public class Error
{
    private string _message;
    private object? _details;

    public Error(string message, object? details = null)
    {
        _message = message;
        _details = details;
    }

    public static IActionResult BadRequest(string message = "Bad Request", object? details = null)
    {
        var error = new Error(message, details);
        return new BadRequestObjectResult(error);
    }

    public static IActionResult NotFound(string message = "Not Found", object? details = null)
    {
        var error = new Error(message, details);
        return new BadRequestObjectResult(error);
    }

    public static IActionResult Unauthorized(string message = "Unauthorized", object? details = null)
    {
        var error = new Error(message, details);
        return new UnauthorizedObjectResult(error);
    }

    public static IActionResult Conflict(string message = "Conflict", object? details = null)
    {
        var error = new Error(message, details);
        return new ConflictObjectResult(error);
    }

    public static IActionResult InternalServerError(string message = "Something went wrong", object? details = null)
    {
        var error = new Error(message, details);
        return new ObjectResult(error)
        {
            StatusCode = (int)HttpStatusCode.InternalServerError
        };
    }

    public static IActionResult Okay(string message = "Okay", object? details = null)
    {
        var error = new Error(message, details);
        return new OkObjectResult(error);
    }
}
