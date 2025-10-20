using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VelocityAPI.Application.DTOs.Auction;

public class Create
{
    [JsonPropertyName("vehicle_id")]
    [StringLength(26, ErrorMessage = "vehicle_id must be 26 characters long.", MinimumLength = 26)]
    [Required(ErrorMessage = "vehicle_id field is required.")]
    public string VehicleId { get; set; } = string.Empty;

    [JsonPropertyName("starting_price")]
    [Required(ErrorMessage = "starting_price field is required.")]
    public decimal StartingPrice { get; set; }
}
