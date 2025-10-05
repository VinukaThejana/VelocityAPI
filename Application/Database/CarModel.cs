using VelocityAPI.Application.DTOs.Car;
using Dapper;
using Npgsql;

namespace VelocityAPI.Application.Database;

public class CarModel
{
    public static async Task AddCar(
      NpgsqlDataSource dataSource,
      AddRequest carData
    )
    {
        const string sql = @"
          INSERT INTO velocity._vehicle (id, owner_id, brand_id, vehicle_type_id, name, is_active, description, photos, model, color, license_plate, year, mileage)
          VALUES(@Id, @OwnerId, @BrandId, @VehicleTypeId, @Name, @IsActive, @Description, @Photos, @Model, @Color, @LicensePlate, @Year, @Mileage);
        ";

        await using var connection = await dataSource.OpenConnectionAsync();

        await connection.QuerySingleAsync(sql, new
        {
            Id = carData.CarId,
        });
    }
}
