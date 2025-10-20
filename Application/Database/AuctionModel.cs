using VelocityAPI.Application.Records.Auction;

using Dapper;
using Npgsql;

namespace VelocityAPI.Application.Database;

public class AuctionModel
{
    public static async Task<AuctionDetails> GetAuctionDetails(
        NpgsqlDataSource dataSource,
        string auctionId
    )
    {
        string sql = @"
        SELECT
          a.status AS Status,
          a.expiration AS Expiration,
          a.start_time AS StartTime,
          
          -- Count the total number of bids
          COUNT(b.id) AS TotalBids,
          
          -- Find the highest bid, or return 0 if there are no bids
          COALESCE(MAX(b.amount), 0) AS HighestBid
        FROM
            velocity._auctions AS a
        LEFT JOIN
            -- Use LEFT JOIN in case an auction has 0 bids
            velocity._bids AS b ON a.id = b.auction_id
        WHERE
            a.id = @AuctionId
        GROUP BY
            -- Group by the auction's primary key and other selected columns
            a.id, a.status, a.expiration, a.start_time;
        ";

        await using var connection = await dataSource.OpenConnectionAsync();

        var auctionDetails = await connection.QuerySingleOrDefaultAsync<AuctionDetails>(
          sql,
          new { AuctionId = auctionId }
        );
        if (auctionDetails == default)
        {
            throw new KeyNotFoundException("Auction not found");
        }
        return auctionDetails;
    }

    public static async Task<BasicAuctionDetails> GetBaisAuctionDetails(
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

        var auctionDetails = await connection.QuerySingleOrDefaultAsync<BasicAuctionDetails>(sql, new { AuctionId = auctionId });
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

