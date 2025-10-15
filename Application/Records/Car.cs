namespace VelocityAPI.Application.Records.Car;

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
