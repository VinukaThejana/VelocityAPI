using VelocityAPI.Application.DTOs.Auction;
using VelocityAPI.Filters;
using VelocityAPI.Models;
using VelocityAPI.Application.Error;
using VelocityAPI.Application.Database;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Npgsql;

namespace VelocityAPI.Controllers.Hello;

[ApiController]
[Route("/api/auction")]
public class AuctionController : ControllerBase
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<AuthController> _logger;

    public AuctionController(
        NpgsqlDataSource dataSource,
        IConnectionMultiplexer redis,
        ILogger<AuthController> logger
    )
    {
        _dataSource = dataSource;
        _redis = redis;
        _logger = logger;
    }

    [HttpGet("{auctionId}")]
    public async Task<IActionResult> GetBasicInfo(
      [FromRoute] string auctionId,
      [FromServices] IOptions<AppSettings> settings
    )
    {
        if (!Ulid.TryParse(auctionId, out var id))
        {
            return Error.BadRequest("Invalid auction ID format.");
        }
        try
        {
            var auctionDetails = await AuctionModel.GetAuctionDetails(_dataSource, auctionId);
            return Ok(new
            {
                status = auctionDetails.Status.ToString(),
                expiration = auctionDetails.Expiration,
                start_time = auctionDetails.StartTime.ToString(),
                total_bids = auctionDetails.TotalBids,
                highest_bid = auctionDetails.HighestBid,
            });
        }
        catch (KeyNotFoundException ex)
        {
            Error.NotFound(ex.Message);
        }

        return Error.InternalServerError("Something went wrong.");
    }

    [HttpPost("create")]
    [AuthorizationFilter]
    public async Task<IActionResult> Upload(
      [FromBody] Create request,
      [FromServices] IOptions<AppSettings> settings
    )
    {
        var userId = HttpContext.Items["UserId"]!.ToString()!;

        var auctionId = await AuctionModel.Create(_dataSource, request.VehicleId, userId, request.StartingPrice);

        return Ok(new
        {
            auction_id = auctionId
        });
    }
}
