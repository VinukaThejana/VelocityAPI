using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace VelocityAPI.Configuration;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisHealthCheck(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            var database = _connectionMultiplexer.GetDatabase();

            var pingDuration = await database.PingAsync();
            if (pingDuration != TimeSpan.Zero)
            {
                return HealthCheckResult.Healthy("Redis connection is healthy");
            }

            return HealthCheckResult.Unhealthy("Redis connection failed to respond to PING");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Redis connection failed", ex);
        }
    }
}
