using Microsoft.Extensions.Options;
using Npgsql;
using VelocityAPI.Models;

namespace VelocityAPI.Configuration;

public static class DatabaseConfig
{
    public static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<NpgsqlDataSource>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<AppSettings>>().Value;

            var dataSourceBuilder = new NpgsqlDataSourceBuilder(settings.DbURL);
            dataSourceBuilder.ConnectionStringBuilder.Pooling = true;
            dataSourceBuilder.ConnectionStringBuilder.MinPoolSize = 5;
            dataSourceBuilder.ConnectionStringBuilder.MaxPoolSize = 50;

            return dataSourceBuilder.Build();
        });

        return services;
    }
}
