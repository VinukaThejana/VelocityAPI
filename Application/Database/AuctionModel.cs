using VelocityAPI.Application.Models;

using Dapper;
using Npgsql;

namespace VelocityAPI.Application.Database;

public class AuctionModel
{
    public static async Task<BaisAuctionDetails> GetBaisAuctionDetails(
      NpgsqlDataSource dataSource,
      string auctionId
    )
    {
        const string sql = @"
        SELECT
            a.status AS Status,
            -- An auction is only active if its status is 'active'
            -- AND its expiration time is still in the future.
            (a.status = 'active' AND a.expiration > EXTRACT(EPOCH FROM NOW())) AS IsActive,
            a.starting_price AS StartingPrice
        FROM
            velocity._auctions AS a
        WHERE
            a.id = @AuctionId;
        ";

        await using var connection = await dataSource.OpenConnectionAsync();

        var auctionDetails = await connection.QuerySingleOrDefaultAsync<BaisAuctionDetails>(sql, new { AuctionId = auctionId });
        if (auctionDetails == default)
        {
            throw new KeyNotFoundException("Auction not found");
        }
        return auctionDetails;
    }

    public static async Task<string> Create(
      NpgsqlDataSource dataSource,
      string vehicleId,
      string sellerId,
      decimal startingPrice
    )
    {
        long expirationTimestamp = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeSeconds();

        const string sql = @"
        INSERT INTO velocity._auctions (
            vehicle_id,
            seller_id,
            starting_price,
            expiration
        )
        VALUES (
            @VehicleId,
            @SellerId,
            @StartingPrice,
            @Expiration
        )
        RETURNING id;
        ";

        await using var connection = await dataSource.OpenConnectionAsync();
        var auctionId = await connection.QuerySingleAsync<string>(sql, new
        {
            VehicleId = vehicleId,
            SellerId = sellerId,
            StartingPrice = startingPrice,
            Expiration = expirationTimestamp
        });

        return auctionId;
    }
}

