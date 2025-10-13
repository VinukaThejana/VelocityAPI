using VelocityAPI.Models;
using VelocityAPI.Application.DTOs.Car;
using VelocityAPI.Application.Models;

using Microsoft.Extensions.Options;
using Dapper;
using Npgsql;

namespace VelocityAPI.Application.Database;

public class CarModel
{
    public static async Task AddCar(
      NpgsqlDataSource dataSource,
      IOptions<AppSettings> settings,
      AddRequest carData,
      string userId
    )
    {
        const string sql = @"
INSERT INTO velocity._vehicle (
  id, owner_id, brand_id, vehicle_type_id, name, photos,
  model, color, license_plate, year, mileage, description,
  features, engine, transmission, estimated_fuel_economy, fuel_capacity,
  horsepower, tourque, drive_train, front_suspension, rear_suspension,
  front_brakes, rear_brakes, wheels, tires, wheelbase, curb_weight,
  length, width, height, zero_to_sixty
)
VALUES (
  @Id, @OwnerId, @BrandId, @VehicleTypeId, @Name, @Photos,
  @Model, @Color, @LicensePlate, @Year, @Mileage, @Description,
  @Features, @Engine, @Transmission, @EstimatedFuelEconomy, @FuelCapacity,
  @Horsepower, @Tourque, @DriveTrain, @FrontSuspension, @RearSuspension,
  @FrontBrakes, @RearBrakes, @Wheels, @Tires, @Wheelbase, @CurbWeight,
  @Length, @Width, @Height, @ZeroToSixty
);";
        await using var connection = await dataSource.OpenConnectionAsync();

        await connection.ExecuteAsync(sql, new
        {
            Id = carData.CarId,
            OwnerId = userId,
            BrandId = carData.BrandId,
            VehicleTypeId = carData.TypeId,
            Name = carData.Name,
            Photos = carData.PhotoIDs,
            Model = carData.Model,
            Color = carData.Color,
            LicensePlate = carData.LicensePlate,
            Year = long.Parse(carData.Year),
            Mileage = carData.Mileage,
            Description = carData.Description,
            Features = carData.Features,
            Engine = carData.Engine,
            Transmission = carData.Transmission,
            EstimatedFuelEconomy = carData.EstimatedFuelEconomy,
            FuelCapacity = carData.FuelCapacity,
            Horsepower = carData.Horsepower,
            Tourque = carData.Tourque,
            DriveTrain = carData.DriveTrain,
            FrontSuspension = carData.FrontSuspension,
            RearSuspension = carData.RearSuspension,
            FrontBrakes = carData.FrontBrakes,
            RearBrakes = carData.RearBrakes,
            Wheels = carData.Wheels,
            Tires = carData.Tires,
            Wheelbase = carData.Wheelbase,
            CurbWeight = carData.CurbWeight,
            Length = carData.Length,
            Width = carData.Width,
            Height = carData.Height,
            ZeroToSixty = carData.ZeroToSixty
        });
    }

    public static async Task<IEnumerable<PopularCar>> GetPopularCars(
      NpgsqlDataSource dataSource,
      IOptions<AppSettings> settings,
      int limit = 10
    )
    {
        const string sql = @"
SELECT
  v.id AS Id,
  v.name AS Name,
  v.model AS Model,
  v.description AS Description,
  v.color AS Color,
  v.photos AS PhotoURLs,
  v.features AS Features,
  v.year AS Year,
  COUNT(b.id) AS BidCount
FROM
    velocity._vehicle AS v
JOIN
    velocity._auctions AS a ON v.id = a.vehicle_id
JOIN
    velocity._bids AS b ON a.id = b.auction_id
GROUP BY
    v.id, v.name, v.model, v.year
ORDER BY
    BidCount DESC
LIMIT @Limit;";

        await using var connection = await dataSource.OpenConnectionAsync();
        var cars = await connection.QueryAsync<PopularCar>(sql, new { Limit = limit });

        List<PopularCar> carList = cars.Select(car => car with
        {
            PhotoURLs = car.PhotoURLs.Select(photoId => $"{settings.Value.AwsBucket}.s3.{settings.Value.AwsRegion}.amazonaws.com/{photoId}").ToList()
        }
        ).ToList();

        return carList;
    }

    public static async Task<IEnumerable<Brand>> GetAllCarBrands(
      NpgsqlDataSource dataSource
    )
    {
        const string sql = @"
SELECT
  id AS Id,
  name AS Name,
  slug AS Slug
FROM velocity._brand
ORDER BY name ASC;";


        await using var connection = await dataSource.OpenConnectionAsync();
        var brands = await connection.QueryAsync<Brand>(sql);

        return brands;
    }

    public static async Task<IEnumerable<PastSaleResult>> GetPastSales(
      NpgsqlDataSource dataSource,
      IOptions<AppSettings> settings,
      int page = 1,
      int pageSize = 10
    )
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        const string sql = @"
SELECT
    v.id AS Id,
    v.name AS Name,
    v.color AS Color,
    v.description AS Description,
    v.photos AS PhotoURLs,
    v.features AS Features,
    v.year AS Year,
    to_timestamp(a.expiration) AS EndedDate,
    t.amount AS SoldForValue
FROM
    velocity._vehicle AS v
JOIN
    velocity._auctions AS a ON v.id = a.vehicle_id
JOIN
    velocity._transactions AS t ON a.id = t.auction_id
WHERE
    a.status = 'sold' AND t.payment_status = 'completed'
ORDER BY
    a.expiration DESC -- Order by the auction end time, most recent first
LIMIT @Limit
OFFSET @Offset;
    ";

        var offset = (page - 1) * pageSize;

        await using var connection = await dataSource.OpenConnectionAsync();

        var sales = await connection.QueryAsync<PastSaleResult>(sql, new
        {
            Limit = pageSize,
            Offset = offset
        });

        List<PastSaleResult> salesList = sales.Select(sale => sale with
        {
            PhotoURLs = sale.PhotoURLs.Select(photoId => $"https://{settings.Value.AwsBucket}.s3.{settings.Value.AwsRegion}.amazonaws.com/{photoId}").ToList()
        }
        ).ToList();

        return salesList;
    }
}

public record PopularCar
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;

    public List<string> PhotoURLs { get; init; } = new();
    public List<string> Features { get; init; } = new();

    public int Year { get; init; }
    public long BidCount { get; init; }
}

public record PastSaleResult
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;

    public List<string> PhotoURLs { get; init; } = new();
    public List<string> Features { get; init; } = new();

    public int Year { get; init; }

    public DateTime EndedDate { get; init; } = new();
    public decimal SoldForValue { get; init; }
}
