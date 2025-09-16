using Microsoft.AspNetCore.Mvc;
using Npgsql;
using VelocityAPI.DTOs.Auth;
using VelocityAPI.Application.Error;
using VelocityAPI.Application.Database;
using VelocityAPI.Models;
using StackExchange.Redis;
using Microsoft.Extensions.Options;
using Resend;

namespace VelocityAPI.Controllers.Hello;

[ApiController]
[Route("/api/auth")]
public class AuthController : ControllerBase
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly IConnectionMultiplexer _redis;

    public AuthController(NpgsqlDataSource dataSource, IConnectionMultiplexer redis)
    {
        _dataSource = dataSource;
        _redis = redis;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Regsiter(
      [FromBody] RegisterRequest request,
      [FromServices] IOptions<AppSettings> settings
    )
    {
        var user = await UserModel.GetUserByEmail(_dataSource, request.Email);
        if (user != null)
        {
            if (user.EmailVerified)
            {
                return Error.Conflict("Email already in use");
            }

            if (user.Strikes == 3)
            {
                return Error.Unauthorized("You have been locked out of the system. This is due to you not buying vehicles after biding on them. Please contact support to unlock your account.");
            }

            await this.SendUserEmailVerificationEmail(user, settings.Value.ResendAPIKey);
            return Error.Okay("Verification email sent");
        }

        var newUser = await UserModel.CreateUser(_dataSource, request);

        await this.SendUserEmailVerificationEmail(newUser, settings.Value.ResendAPIKey);
        return Error.Okay("Verification email sent");
    }


    private string GetUserEmailVerificationRedisKey(string token) => $"email_verification:{token}";

    private async Task SendUserEmailVerificationEmail(
        User user,
        string ResendAPIKey
    )
    {
        var request = HttpContext.Request;

        var verificationToken = Ulid.NewUlid().ToString();

        var redisDb = _redis.GetDatabase();
        var redisKey = this.GetUserEmailVerificationRedisKey(verificationToken);
        await redisDb.StringSetAsync(redisKey, user.Id, TimeSpan.FromHours(1));

        var verificationLink = $"{request.Scheme}://{request.Host}/api/auth/verify/email?token={verificationToken}";

        var templatePath = Path.Combine("Templates", "Email", "User", "Verify.html");
        var emailBody = await System.IO.File.ReadAllTextAsync(templatePath);

        emailBody = emailBody.Replace("{{user_name}}", user.Name);
        emailBody = emailBody.Replace("{{verification_link}}", verificationLink);

        var resend = ResendClient.Create(ResendAPIKey);

        var resp = await resend.EmailSendAsync(new EmailMessage()
        {
            From = "Velocity API<onboarding@vinuka.dev>",
            To = user.Email,
            Subject = "Verify your email address",
            HtmlBody = emailBody
        });

        Console.WriteLine($"Email Id={resp.Content}");
    }
}

