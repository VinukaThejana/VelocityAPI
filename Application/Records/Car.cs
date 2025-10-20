namespace VelocityAPI.Application.Records.Car;

public record Car
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

public record Brand
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

public record PopularCar
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;

    public string[] PhotoURLs { get; init; } = Array.Empty<string>();
    public string[] Features { get; init; } = Array.Empty<string>();

    public int Year { get; init; }
    public long BidCount { get; init; }
}

public record PastSaleResult
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;

    public string[] PhotoURLs { get; init; } = Array.Empty<string>();
    public string[] Features { get; init; } = Array.Empty<string>();

    public int Year { get; init; }

    public DateTime EndedDate { get; init; } = new();
    public decimal SoldForValue { get; init; }
}

public record BasicCarDetail
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string BrandName { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;

    public int Year { get; init; }

    public string[] PhotoURLs { get; init; } = Array.Empty<string>();
}

public record CarBrand
{
    public short Id { get; set; }

    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public record CarType
{
    public short Id { get; set; }

    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
