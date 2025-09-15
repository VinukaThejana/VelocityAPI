using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace VelocityAPI.Application.Error;

public class Error
{
    public string Message { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Payload { get; }

    public Error(string message, object? details = null)
    {
        Message = message;
        Payload = details;
    }

    public static IActionResult BadRequest(string message = "Bad Request", object? details = null)
      => new BadRequestObjectResult(new Error(message, details));

    public static IActionResult NotFound(string message = "Not Found", object? details = null)
      => new NotFoundObjectResult(new Error(message, details));

    public static IActionResult Unauthorized(string message = "Unauthorized", object? details = null)
      => new UnauthorizedObjectResult(new Error(message, details));

    public static IActionResult Conflict(string message = "Conflict", object? details = null)
      => new ConflictObjectResult(new Error(message, details));

    public static IActionResult InternalServerError(string message = "Something went wrong", object? details = null)
      => new ObjectResult(new Error(message, details)) { StatusCode = (int)HttpStatusCode.InternalServerError };

    public static IActionResult Okay(string message = "Okay", object? details = null)
      => new OkObjectResult(new Error(message, details));
}
