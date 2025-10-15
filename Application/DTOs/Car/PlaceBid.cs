using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VelocityAPI.Application.DTOs.Car;

public class PlaceBid
{
    [JsonPropertyName("auction_id")]
    [Required(ErrorMessage = "The auction_id field is required.")]
    [StringLength(26, MinimumLength = 26, ErrorMessage = "The auction_id field must be exactly 26 characters long.")]
    public string AuctionId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    [Required(ErrorMessage = "The amount field is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "The amount field must be greater than 0.")]
    public decimal Amount { get; set; }
}
