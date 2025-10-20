using VelocityAPI.Application.DTOs.Auth;
using VelocityAPI.Application.Records.User;

using Dapper;
using Npgsql;

namespace VelocityAPI.Application.Database;

public class UserModel
{
    public static async Task<User?> GetUserByEmail(
      NpgsqlDataSource dataSource,
      string email
    )
    {
        const string sql = @"
        SELECT
          id AS Id,
          email AS Email,
          name AS Name,
          photo_url AS PhotoUrl,
          nic AS Nic,
          password AS PasswordHash,
          strikes AS Strikes,
          email_verified AS EmailVerified
        FROM velocity._user
        WHERE email = @email
      ";

        await using var connection = await dataSource.OpenConnectionAsync();

        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { email = email });
    }

    public static async Task<User?> GetUserById(
      NpgsqlDataSource dataSource,
      string id
    )
    {
        const string sql = @"
        SELECT
          id AS Id,
          email AS Email,
          name AS Name,
          photo_url AS PhotoUrl,
          nic AS Nic,
          password AS PasswordHash,
          strikes AS Strikes,
          email_verified AS EmailVerified
        FROM velocity._user
        WHERE id = @id
      ";

        await using var connection = await dataSource.OpenConnectionAsync();

        return await connection.QuerySingleOrDefaultAsync<User>(sql, new { id = id });
    }

    public static async Task<User> CreateUser(
      NpgsqlDataSource dataSource,
      RegisterRequest userData
    )
    {
        const string sql = @"
          INSERT INTO velocity._user (email, name, photo_url, password, nic)
          VALUES (@Email, @Name, @PhotoUrl, @Password, @Nic)
          RETURNING
            id AS Id,
            email AS Email,
            name AS Name,
            photo_url AS PhotoUrl,
            nic AS Nic,
            strikes AS Strikes,
            email_verified AS EmailVerified;
        ";

        await using var connection = await dataSource.OpenConnectionAsync();

        var newUser = await connection.QuerySingleAsync<User>(sql, new
        {
            Email = userData.Email,
            Name = userData.Name,
            PhotoUrl = userData.getPhotoUrl(),
            Password = userData.getHashedPassword(),
            Nic = userData.Nic
        });

        return newUser;
    }

    public static async Task MarkEmailAsVerified(
      NpgsqlDataSource dataSource,
      string userId
    )
    {
        const string sql = @"
          UPDATE velocity._user
          SET email_verified = TRUE
          WHERE id = @userId
        ";

        await using var connection = await dataSource.OpenConnectionAsync();
        await connection.ExecuteAsync(sql, new { userId = userId });
    }
}
