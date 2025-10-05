using VelocityAPI.Application.DTOs.Car;
using VelocityAPI.Filters;
using VelocityAPI.Models;
using VelocityAPI.Application.Constants;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using StackExchange.Redis;
using Npgsql;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace VelocityAPI.Controllers.Hello;

[ApiController]
[Route("/api/car")]
public class CarController : ControllerBase
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<AuthController> _logger;

    public CarController(
        NpgsqlDataSource dataSource,
        IConnectionMultiplexer redis,
        ILogger<AuthController> logger
    )
    {
        _dataSource = dataSource;
        _redis = redis;
        _logger = logger;
    }

    [HttpPost("images/upload")]
    [AuthorizationFilter]
    public async Task<IActionResult> Upload(
      [FromBody] UploadRequest request,
      [FromServices] IOptions<AppSettings> settings
    )
    {
        var carId = Ulid.NewUlid().ToString();

        var fileNames = new List<string>();
        foreach (var fileName in request.FileNames)
        {
            string uniqueFileName = $"{Ulid.NewUlid().ToString()}_{Regex.Replace(fileName, @"(?!\.[^.]+$)[^a-zA-Z0-9]+", "-")}";
            fileNames.Add(uniqueFileName);
        }

        var s3Client = new AmazonS3Client(
          settings.Value.AwsAccessKeyId,
          settings.Value.AwsAccessKeySecret,
          RegionEndpoint.GetBySystemName(settings.Value.AwsRegion)
        );

        var urls = new List<string>();
        foreach (var fileName in fileNames)
        {
            var url = await s3Client.GetPreSignedURLAsync(new GetPreSignedUrlRequest
            {
                BucketName = settings.Value.AwsBucket,
                Key = $"cars/{carId}/{fileName}",
                Expires = DateTime.UtcNow.AddMinutes(20),
                Verb = HttpVerb.PUT
            });
            urls.Add(url);
        }

        var redis = _redis.GetDatabase();

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var result = await redis.SetAddAsync(
          Redis.CarUploadPendingSetKey,
          $"{carId}:{now}"
        );
        if (!result)
        {
            throw new Exception("Failed to add car upload id to the redis set");
        }

        return Ok(new
        {
            car_id = carId,
            urls = urls,
        });
    }
}
