using VelocityAPI.Models;
using VelocityAPI.Application.Error;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace VelocityAPI.Filters;

public class CronJobFilter : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var _settings = context.HttpContext.RequestServices.GetRequiredService<IOptions<AppSettings>>();

        if (context.HttpContext.Request.Headers.TryGetValue("Authorization", out var authHeaderValue))
        {
            var authHeader = authHeaderValue.ToString();
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var secret = authHeader.Substring("Bearer ".Length).Trim();
                if (!string.IsNullOrEmpty(secret) && secret == _settings.Value.RouteSecret)
                {
                    await next();
                    return;
                }
            }
        }

        context.Result = new UnauthorizedObjectResult(
            new Error("This route should only be accessed by a cron job")
        );
        return;

    }
}
