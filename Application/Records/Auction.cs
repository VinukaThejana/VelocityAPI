namespace VelocityAPI.Application.Records.Auction;


public record AuctionDetails
{
    public string Status { get; init; } = string.Empty;
    public long Expiration { get; init; }
    public DateTime StartTime { get; init; }
    public int TotalBids { get; init; }
    public decimal HighestBid { get; init; }
}

public record BasicAuctionDetails
{
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; init; }
    public decimal StartingPrice { get; init; }
}
