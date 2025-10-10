using VelocityAPI.Application.S3;
using VelocityAPI.Models;
using VelocityAPI.Application.Constants;
using VelocityAPI.Filters;

using StackExchange.Redis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Npgsql;
using Amazon;
using Amazon.S3;

[ApiController]
[Route("/api/cron")]
public class CronController : ControllerBase
{

    private readonly NpgsqlDataSource _dataSource;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<ControllerBase> _logger;

    public CronController(
      NpgsqlDataSource dataSource,
      IConnectionMultiplexer redis,
      ILogger<ControllerBase> logger
    )
    {
        _dataSource = dataSource;
        _redis = redis;
        _logger = logger;
    }


    class CarUploadPendingItem
    {
        public string Id { get; set; } = string.Empty;
        public string CarId { get; set; } = string.Empty;
    }

    [HttpPatch("car/images/cleanup")]
    [CronJobFilter]
    public async Task<IActionResult> CarImagesCleanup(
      [FromServices] IOptions<AppSettings> settings
    )
    {

        var redis = _redis.GetDatabase();
        var server = _redis.GetServer(_redis.GetEndPoints().First());

        int pageSize = 20;
        var keys = new List<RedisKey>();

        await foreach (var key in server.KeysAsync(pattern: $"{Redis.CarUploadPendingKey}:*", pageSize: pageSize))
        {
            keys.Add(key);
            if (keys.Count >= pageSize) break;
        }
        if (keys.Count == 0)
        {
            _logger.LogInformation("No car upload pending items to delete");
            return Ok(new { success = true, deleted = 0 });
        }

        var carIds = new List<string>();
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        RedisValue[] values = await redis.StringGetAsync(keys.ToArray());
        for (int i = 0; i < keys.Count; i++)
        {
            if (string.IsNullOrEmpty(values[i])) continue;
            if (string.IsNullOrEmpty(keys[i])) continue;

            if (!long.TryParse(values[i].ToString().Trim(), out var timestamp)) continue;
            if ((now - timestamp) < 3600) continue; // should give the user at least 1 hour to finish the upload

            var keyParts = keys[i].ToString().Split($"{Redis.CarUploadPendingKey}:");
            if (keyParts.Length != 2) continue;
            var carId = keyParts[1];

            carIds.Add(carId);
        }

        var s3Client = new AmazonS3Client(
          settings.Value.AwsAccessKeyId,
          settings.Value.AwsAccessKeySecret,
          RegionEndpoint.GetBySystemName(settings.Value.AwsRegion)
        );

        var deleteTasks = carIds.Select(
            async carId =>
            {
                await S3.DeleteS3FolderAsync(
                  s3Client,
                  settings.Value.AwsBucket,
                  $"cars/{carId}/"
                );
                await redis.KeyDeleteAsync($"{Redis.CarUploadPendingKey}:{carId}");
            }
        ).ToList();

        await Task.WhenAll(deleteTasks);

        _logger.LogInformation("Deleted {Count} car upload pending items", carIds.Count);
        return Ok(new { success = true, deleted = carIds.Count });
    }
}
