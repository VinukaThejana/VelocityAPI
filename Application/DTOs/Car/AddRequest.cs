using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VelocityAPI.Application.DTOs.Car;

public class AddRequest
{
    [JsonPropertyName("car_id")]
    [Required(ErrorMessage = "The car_id field is required.")]
    public string CarId { get; set; } = string.Empty;

    [JsonPropertyName("brand_id")]
    [Required(ErrorMessage = "The brand_id field is required.")]
    public int BrandId { get; set; } = -1;

    [JsonPropertyName("type_id")]
    [Required(ErrorMessage = "The type_id field is required.")]
    public int TypeId { get; set; } = -1;

    [JsonPropertyName("model")]
    [Required(ErrorMessage = "The model field is required.")]
    [MinLength(3, ErrorMessage = "The model field must be at least 3 characters long.")]
    [MaxLength(255, ErrorMessage = "The model field must be at most 255 characters long.")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("photo_ids")]
    [Required(ErrorMessage = "The photo_ids field is required.")]
    public List<string> PhotoIDs { get; set; } = new List<string>();

    [JsonPropertyName("color")]
    [Required(ErrorMessage = "The color field is required.")]
    [MinLength(3, ErrorMessage = "The color field must be at least 3 characters long.")]
    [MaxLength(100, ErrorMessage = "The color field must be at most 100 characters long.")]
    public string Color { get; set; } = string.Empty;

    [JsonPropertyName("license_plate")]
    [Required(ErrorMessage = "The license_plate field is required.")]
    [MinLength(5, ErrorMessage = "The license_plate field must be at least 5 characters long.")]
    [MaxLength(15, ErrorMessage = "The license_plate field must be at most 15 characters long.")]
    public string LicensePlate { get; set; } = string.Empty;

    [JsonPropertyName("year")]
    [Required(ErrorMessage = "The year field is required.")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "The year field must be a valid 4-digit year.")]
    public string year { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    [Required(ErrorMessage = "The description field is required.")]
    [MinLength(10, ErrorMessage = "The description field must be at least 10 characters long.")]
    [MaxLength(1000, ErrorMessage = "The description field must be at most 1000 characters long.")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("mileage")]
    [Required(ErrorMessage = "The mileage field is required.")]
    [Range(0, int.MaxValue, ErrorMessage = "The mileage field must be a non-negative integer.")]
    public int Mileage { get; set; } = -1;

    [JsonPropertyName("name")]
    [Required(ErrorMessage = "The name field is required.")]
    [MinLength(2, ErrorMessage = "The name field must be at least 2 characters long.")]
    [MaxLength(50, ErrorMessage = "The name field must be at most 50 characters long.")]
    public string Name { get; set; } = string.Empty;
}
