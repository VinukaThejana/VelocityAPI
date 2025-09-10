using Microsoft.Extensions.Options;
using VelocityAPI.Models;

namespace VelocityAPI.Configuration;

public static class RedisConfig
{
    public static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<AppSettings>>().Value;
            return StackExchange.Redis.ConnectionMultiplexer.Connect(settings.RedisURL);
        });

        services.AddStackExchangeRedisCache(options =>
        {
            options.InstanceName = "velocityapi:";
        });

        return services;
    }
}

