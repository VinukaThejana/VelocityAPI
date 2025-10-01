using VelocityAPI.Models;
using VelocityAPI.Application.Error;
using VelocityAPI.Application.Authentication.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace VelocityAPI.Filters;

public class AuthorizationFilter : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var _redis = context.HttpContext.RequestServices.GetRequiredService<IConnectionMultiplexer>();
        var _settings = context.HttpContext.RequestServices.GetRequiredService<IOptions<AppSettings>>();

        if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeaderValue))
        {
            var authHeader = authHeaderValue.ToString();
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                if (!string.IsNullOrEmpty(token))
                {
                    var access = new Access(_redis, _settings);
                    var claims = await access.Verify(token);

                    context.HttpContext.Items["UserId"] = claims.UserId;
                    await next();
                    return;
                }
            }
        }

        context.Result = new UnauthorizedObjectResult(
            new Error("You are not authorized to access this resource.")
        );
        return;

    }
}
