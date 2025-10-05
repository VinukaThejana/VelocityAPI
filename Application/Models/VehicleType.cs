namespace VelocityAPI.Application.Models;

public class VehicleType
{
    public short Id { get; set; }

    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
