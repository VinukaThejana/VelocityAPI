using Microsoft.AspNetCore.Mvc;
using Npgsql;
using VelocityAPI.DTOs.Auth;
using VelocityAPI.Application.Error;
using VelocityAPI.Application.Database;
using VelocityAPI.Models;
using StackExchange.Redis;
using Microsoft.Extensions.Options;
using Resend;
using System.Net;

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

    [HttpGet("verify/email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        try
        {
            if (string.IsNullOrEmpty(token))
            {
                return this.RedirectToPage("Please signup to send a verification email", 400);
            }

            var redisDb = _redis.GetDatabase();
            var redisKey = this.GetUserEmailVerificationRedisKey(token);

            var userId = (string?)await redisDb.StringGetAsync(redisKey);
            if (string.IsNullOrEmpty(userId) || userId == RedisValue.Null)
            {
                return this.RedirectToPage("Invalid or expired token, Please signup again", 400);
            }

            _ = redisDb.KeyDeleteAsync(redisKey);

            var user = await UserModel.GetUserById(_dataSource, userId);
            if (user == null)
            {
                return this.RedirectToPage("User not found, Please signup again", 404);
            }
            if (user.EmailVerified)
            {
                return this.RedirectToPage("Email already verified, Please login", 200);
            }
            if (user.Strikes == 3)
            {
                return this.RedirectToPage("You have been locked out of the system. This is due to you not buying vehicles after biding on them. Please contact support to unlock your account.", 401);
            }

            await UserModel.MarkEmailAsVerified(_dataSource, user.Id);

            return this.RedirectToPage("", 200);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return this.RedirectToPage("Something went wrong. Please try again later", 500);
        }
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

    private IActionResult RedirectToPage(string errorMessage, int statusCode)
    {
        // NOTE: CHANGE THIS TO YOUR PRODUCTION FRONTEND URL
        const string baseUrl = "http://localhost:3000";

        if (statusCode == 200 && errorMessage == "")
        {
            return Redirect($"{baseUrl}/login");
        }

        var encodedMessage = WebUtility.UrlEncode(errorMessage);
        var redirectUrl = $"{baseUrl}/sign-up?status={statusCode}&error={encodedMessage}";

        return Redirect(redirectUrl);
    }
}

