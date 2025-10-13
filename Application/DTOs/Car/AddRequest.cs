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
    public List<string> PhotoIDs { get; set; } = new();

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
    public string Year { get; set; } = string.Empty;

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

    [JsonPropertyName("features")]
    public List<string> Features { get; set; } = new();

    [JsonPropertyName("engine")]
    [Required(ErrorMessage = "The engine field is required.")]
    public string Engine { get; set; } = string.Empty;

    [JsonPropertyName("transmission")]
    [Required(ErrorMessage = "The transmission field is required.")]
    public string Transmission { get; set; } = string.Empty;

    [JsonPropertyName("estimated_fuel_economy")]
    [Required(ErrorMessage = "The estimated_fuel_economy field is required.")]
    public string EstimatedFuelEconomy { get; set; } = string.Empty;

    [JsonPropertyName("fuel_capacity")]
    [Required(ErrorMessage = "The fuel_capacity field is required.")]
    public string FuelCapacity { get; set; } = string.Empty;

    [JsonPropertyName("horsepower")]
    public string Horsepower { get; set; } = string.Empty;

    [JsonPropertyName("tourque")]
    public string Tourque { get; set; } = string.Empty;

    [JsonPropertyName("drive_train")]
    public string DriveTrain { get; set; } = string.Empty;

    [JsonPropertyName("front_suspension")]
    public string FrontSuspension { get; set; } = string.Empty;

    [JsonPropertyName("rear_suspension")]
    public string RearSuspension { get; set; } = string.Empty;

    [JsonPropertyName("front_brakes")]
    public string FrontBrakes { get; set; } = string.Empty;

    [JsonPropertyName("rear_brakes")]
    public string RearBrakes { get; set; } = string.Empty;

    [JsonPropertyName("wheels")]
    public string Wheels { get; set; } = string.Empty;

    [JsonPropertyName("tires")]
    public string Tires { get; set; } = string.Empty;

    [JsonPropertyName("wheelbase")]
    public string Wheelbase { get; set; } = string.Empty;

    [JsonPropertyName("curb_weight")]
    public string CurbWeight { get; set; } = string.Empty;

    [JsonPropertyName("length")]
    public string Length { get; set; } = string.Empty;

    [JsonPropertyName("width")]
    public string Width { get; set; } = string.Empty;

    [JsonPropertyName("height")]
    public string Height { get; set; } = string.Empty;

    [JsonPropertyName("zero_to_sixty")]
    public decimal ZeroToSixty { get; set; } = -1;
}
