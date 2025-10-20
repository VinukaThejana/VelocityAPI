namespace VelocityAPI.Application.Models;

public class BaisAuctionDetails
{
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; init; }
    public decimal StartingPrice { get; init; }
}
