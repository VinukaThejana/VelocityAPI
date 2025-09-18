using Microsoft.Extensions.Options;
using VelocityAPI.Models;
using VelocityAPI.Configuration;
using VelocityAPI.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services
  .AddOptions<AppSettings>()
  .Bind(builder.Configuration)
  .ValidateDataAnnotations()
  .ValidateOnStart();

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddRedis(builder.Configuration);

builder.Services.AddHealthChecks()
  .AddCheck<DataSourceHealthCheck>("PostgreSQL")
  .AddCheck<RedisHealthCheck>("Redis");

builder.Services.AddControllers();


var app = builder.Build();

app.UseGlobalExceptionHandler();
app.UseHttpsRedirection();

app.MapControllers();
app.MapHealthChecks("/api/health");

var settings = app.Services.GetRequiredService<IOptions<AppSettings>>().Value;
app.Urls.Add($"http://*:{settings.Port}");

app.Run();
