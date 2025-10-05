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

        int pageSize = 20;
        var items = new List<CarUploadPendingItem>();

        await foreach (var item in redis.SetScanAsync(Redis.CarUploadPendingSetKey, pageSize: pageSize))
        {
            var Id = item.ToString();
            if (string.IsNullOrEmpty(Id))
            {
                continue;
            }

            var parts = Id.Split(':');
            if (parts.Length != 2)
            {
                continue;
            }

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var carId = parts[0];
            if (!long.TryParse(parts[1], out var timestamp))
            {
                continue;
            }
            if (now - timestamp < 3600) // 1 hour
            {
                continue;
            }

            items.Add(new CarUploadPendingItem
            {
                Id = Id,
                CarId = carId
            });

            if (items.Count >= pageSize)
            {
                break;
            }
        }

        if (items.Count == 0)
        {
            _logger.LogInformation("No car upload pending items to delete");
            return Ok(new { success = true, deleted = 0 });
        }

        var s3Client = new AmazonS3Client(
          settings.Value.AwsAccessKeyId,
          settings.Value.AwsAccessKeySecret,
          RegionEndpoint.GetBySystemName(settings.Value.AwsRegion)
        );

        var deleteTasks = items.Select(
            async item =>
            {
                await S3.DeleteS3FolderAsync(
                  s3Client,
                  settings.Value.AwsBucket,
                  $"cars/{item.CarId}/"
                );
                await redis.SetRemoveAsync(
                  Redis.CarUploadPendingSetKey,
                  item.Id
                );
            }
        ).ToList();

        await Task.WhenAll(deleteTasks);

        _logger.LogInformation("Deleted {Count} car upload pending items", items.Count);
        return Ok(new { success = true, deleted = items.Count });
    }
}
