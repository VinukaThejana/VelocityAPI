namespace VelocityAPI.Application.Models;

public class Car
{
    public string Id { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Photos { get; set; } = new();

    public short BrandId { get; set; }
    public short VehicleTypeId { get; set; }

    public int Year { get; set; }
    public int Mileage { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
