using VelocityAPI.Models;
using VelocityAPI.Application.DTOs.Car;

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
          model, color, license_plate, year, mileage, description
        )
        VALUES (
          @Id, @OwnerId, @BrandId, @VehicleTypeId, @Name, @Photos,
          @Model, @Color, @LicensePlate, @Year, @Mileage, @Description
        );";
        await using var connection = await dataSource.OpenConnectionAsync();

        var urls = new List<string>();
        foreach (var data in carData.PhotoIDs)
        {
            urls.Append(data.Key);
        }

        await connection.QuerySingleAsync(sql, new
        {
            Id = carData.CarId,
            OwnerId = userId,
            BrandId = carData.BrandId,
            VehicleTypeId = carData.TypeId,
            Name = carData.Name,
            Photos = carData.PhotoIDs.Select(data => data.Key).ToList(),
            Model = carData.Model,
            Color = carData.Color,
            LicensePlate = carData.LicensePlate,
            Year = carData.Year,
            Mileage = carData.Mileage,
            Description = carData.Description
        });
    }
}
