using System.Net;
using VelocityAPI.Application.Error;
using VelocityAPI.Application.Authentication.Errors;
using Npgsql;

namespace VelocityAPI.Middlewares;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    public async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = 500;
        var message = "";

        Console.WriteLine("The exception occurs here");

        switch (exception)
        {
            case TokenValidationException:
                statusCode = (int)HttpStatusCode.Unauthorized;
                message = "Please logout and login again.";
                break;
            case TokenInternalException:
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "Something went wrong. Please try again later.";
                break;

            case PostgresException pgEx when pgEx.SqlState == "23505":
                statusCode = (int)HttpStatusCode.Conflict;
                message = "Conflict occurred. Duplicate entry.";
                break;
            case PostgresException pgEx when pgEx.SqlState == "23503":
                statusCode = (int)HttpStatusCode.BadRequest;
                message = "Invalid request. Please check your input and try again.";
                break;
            case PostgresException pgEx when pgEx.SqlState == "23502":
                statusCode = (int)HttpStatusCode.BadRequest;
                message = "Missing required field. Please check your input and try again.";
                break;

            default:
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "Something went wrong. Please try again later.";
                break;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var errorDetails = _env.IsDevelopment()
          ? exception.ToString()
          : message;

        var response = new Error(errorDetails);
        await context.Response.WriteAsJsonAsync(response);
    }
}
