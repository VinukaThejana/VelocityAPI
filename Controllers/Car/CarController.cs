using VelocityAPI.Application.DTOs.Car;
using VelocityAPI.Filters;
using VelocityAPI.Models;
using VelocityAPI.Application.Constants;
using VelocityAPI.Application.Error;
using VelocityAPI.Application.Database;
using VelocityAPI.Application.Exceptions.Car;

using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
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

        var data = new List<URL>();
        foreach (var fileName in fileNames)
        {
            var url = await s3Client.GetPreSignedURLAsync(new GetPreSignedUrlRequest
            {
                BucketName = settings.Value.AwsBucket,
                Key = $"cars/{carId}/{fileName}",
                Expires = DateTime.UtcNow.AddMinutes(20),
                Verb = HttpVerb.PUT
            });
            data.Add(new URL
            {
                Url = url,
                Key = $"cars/{carId}/{fileName}"
            });
        }

        var redis = _redis.GetDatabase();

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var result = await redis.StringSetAsync($"{Redis.CarUploadPendingKey}:{carId}", now.ToString());
        if (!result)
        {
            _logger.LogError("Failed to set Redis key for car upload pending: {CarId}", carId);
            return StatusCode(500, new { message = "Internal server error" });
        }

        return Ok(new
        {
            car_id = carId,
            data = data,
        });
    }

    [HttpPost("add")]
    [AuthorizationFilter]
    public async Task<IActionResult> Add(
      [FromBody] AddRequest request,
      [FromServices] IOptions<AppSettings> settings
    )
    {
        var userId = HttpContext.Items["UserId"]!.ToString()!;

        var redis = _redis.GetDatabase();
        var redisKey = $"{Redis.CarUploadPendingKey}:{request.CarId}";
        var result = await redis.StringGetAsync(redisKey);
        if (string.IsNullOrEmpty(result) || result == RedisValue.Null)
        {
            _logger.LogWarning("Car upload session expired or does not exist: {CarId}", request.CarId);
            return Error.BadRequest("Car upload session expired or does not exist");
        }

        await CarModel.AddCar(_dataSource, settings, request, userId);
        await redis.KeyDeleteAsync(redisKey);

        return Error.Okay("Car added successfully");
    }

    [HttpPost("bid/start")]
    [AuthorizationFilter]
    public async Task<IActionResult> StartBid(
      [FromBody] PlaceBid request,
      [FromServices] IOptions<AppSettings> settings
    )
    {
        var userId = HttpContext.Items["UserId"]!.ToString()!;
        var appDomain = Env.IsDevelopment(settings.Value.Environment)
          ? $"http://localhost:{settings.Value.Port}"
          : $"https://{settings.Value.Domain}";
        var frontendUrl = settings.Value.FrontendUrl.EndsWith("/")
          ? settings.Value.FrontendUrl[..^1]
          : settings.Value.FrontendUrl;

        var redis = _redis.GetDatabase();
        var highestBid = await redis.StringGetAsync($"{Redis.HighestBidKey}:{request.AuctionId}");
        if (highestBid != RedisValue.Null && decimal.TryParse(highestBid, out var highestBidAmount))
        {
            if (request.Amount <= highestBidAmount)
            {
                return Error.BadRequest($"Bid amount must be higher than the current highest bid of {highestBidAmount:C}");
            }
        }

        try
        {
            var details = await AuctionModel.GetBaisAuctionDetails(_dataSource, request.AuctionId);
            if (details.IsActive == false || details.Status != "active")
            {
                return Error.BadRequest("Auction is not active");
            }
            if (request.Amount < details.StartingPrice)
            {
                return Error.BadRequest($"Bid amount must be at least the starting price of {details.StartingPrice:C}");
            }
        }
        catch (KeyNotFoundException ex)
        {
            return Error.NotFound(ex.Message);
        }

        var client = new StripeClient(settings.Value.StripeApiKey);
        var options = new SessionCreateOptions
        {
            Mode = "payment",
            PaymentMethodTypes = new List<string> { "card" },
            LineItems = new List<SessionLineItemOptions>
          {
            new()
            {
              PriceData = new SessionLineItemPriceDataOptions
              {
                Currency = "lkr",
                UnitAmount = 1000,
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                  Name = "Verification Fee",
                  Description = "Fee to verify your identity before placing a bid. This amount will not be refunded."
                }
              },
              Quantity = 1
            }
          },
            Metadata = new Dictionary<string, string>
          {
            { "user_id", userId},
            { "auction_id", request.AuctionId},
            { "bid_amount", request.Amount.ToString("0.00") }
          },
            SuccessUrl = $"{appDomain}/api/car/bid?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = $"{frontendUrl}/auction/{request.AuctionId}?payment=failed"
        };

        var service = new SessionService(client);
        var session = await service.CreateAsync(options);

        return Ok(new
        {
            checkout_url = session.Url,
            session_id = session.Id
        });
    }

    [HttpPost("bid")]
    public async Task<IActionResult> Bid(
      [FromServices] IOptions<AppSettings> settings
    )
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();

        Event stripEvent;
        try
        {
            stripEvent = EventUtility.ConstructEvent(
              json,
              signature,
              settings.Value.StripeApiKey
            );
        }
        catch (StripeException ex)
        {
            _logger.LogError("Stripe webhook signature verification failed: {Message}", ex.Message);
            return Error.BadRequest("Invalid signature");
        }

        if (stripEvent.Type != "checkout.session.completed")
        {
            return Error.BadRequest("Invalid event type");
        }

        var session = stripEvent.Data.Object as Stripe.Checkout.Session;
        if (session == null)
        {
            _logger.LogError("Stripe webhook session object is null");
            return Error.BadRequest("Invalid session data");
        }

        var ok = false;
        ok = session.Metadata.TryGetValue("user_id", out var userId);
        ok = session.Metadata.TryGetValue("auction_id", out var auctionId);
        ok = session.Metadata.TryGetValue("bid_amount", out var bidAmountStr);
        if (!ok || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(auctionId) || string.IsNullOrEmpty(bidAmountStr))
        {
            _logger.LogError("Missing metadata in Stripe session");
            return Error.BadRequest("Missing metadata");
        }
        if (!decimal.TryParse(bidAmountStr, out var bidAmount))
        {
            _logger.LogError("Invalid bid amount in Stripe metadata: {BidAmount}", bidAmountStr);
            return Error.BadRequest("Invalid bid amount");
        }

        var redis = _redis.GetDatabase();

        var lockKey = $"{Redis.AuctionLockKey}:{auctionId}";
        var lockToken = Ulid.NewUlid().ToString();

        if (await redis.LockTakeAsync(lockKey, lockToken, TimeSpan.FromSeconds(30)))
        {
            try
            {
                await CarModel.PlaceBid(_dataSource, auctionId, userId, bidAmount);
                await redis.StringSetAsync($"{Redis.HighestBidKey}:{auctionId}", bidAmount.ToString());

                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError("Auction not found: {Message}", ex.Message);
                return Ok();
            }
            catch (Exception ex) when (ex is AuctionNotActiveException or BidTooLowException)
            {
                _logger.LogError("Error placing bid: {Message}", ex.Message);
                return Ok();
            }
            finally
            {
                await redis.LockReleaseAsync(lockKey, lockToken);
            }
        }
        else
        {
            return Error.Conflict("Another bid is being processed for this auction. Please try again.");
        }
    }

    [HttpGet("list/popular")]
    public async Task<IActionResult> ListPopular(
      [FromServices] IOptions<AppSettings> settings,
      [FromQuery] int limit = 10
    )
    {
        var cars = await CarModel.GetPopularCars(_dataSource, settings, limit);
        return Ok(new { data = cars });
    }

    [HttpGet("list/brands")]
    public async Task<IActionResult> ListBrands()
    {
        var brands = await CarModel.GetAllCarBrands(_dataSource);
        return Ok(new { data = brands });
    }

    [HttpGet("list/pastsales")]
    public async Task<IActionResult> ListPastSales(
      [FromServices] IOptions<AppSettings> settings,
      [FromQuery] int limit = 10,
      [FromQuery] int page = 1
    )
    {
        var sales = await CarModel.GetPastSales(_dataSource, settings, page, limit);
        return Ok(new
        {
            data = sales,
            page = page,
            page_size = limit
        });
    }

    [HttpGet("list/by-brand")]
    public async Task<IActionResult> GetCarsByBrandId(
      [FromServices] IOptions<AppSettings> settings,
      [FromQuery] string slug)
    {
        var cars = await CarModel.GetCarDetailsByBrand(_dataSource, settings, slug);
        return Ok(new { data = cars });
    }
}
