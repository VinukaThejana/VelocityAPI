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

        switch (exception)
        {
            case TokenValidationException tve:
                _logger.LogError(tve, "Token validation failed.");
                statusCode = (int)HttpStatusCode.Unauthorized;
                message = "Please logout and login again.";
                break;
            case TokenInternalException tie:
                _logger.LogError(tie, "Token internal error.");
                statusCode = (int)HttpStatusCode.InternalServerError;
                message = "Something went wrong. Please try again later.";
                break;

            case PostgresException pgEx when pgEx.SqlState == "23505":
                _logger.LogError(pgEx, "Database conflict error.");
                statusCode = (int)HttpStatusCode.Conflict;
                message = "Conflict occurred. Duplicate entry.";
                break;
            case PostgresException pgEx when pgEx.SqlState == "23503":
                _logger.LogError(pgEx, "Foreign key violation error.");
                statusCode = (int)HttpStatusCode.BadRequest;
                message = "Invalid request. Please check your input and try again.";
                break;
            case PostgresException pgEx when pgEx.SqlState == "23502":
                _logger.LogError(pgEx, "Not null violation error.");
                statusCode = (int)HttpStatusCode.BadRequest;
                message = "Missing required field. Please check your input and try again.";
                break;

            default:
                _logger.LogError(exception, "An unexpected error occurred.");
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
