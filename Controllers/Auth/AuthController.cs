using VelocityAPI.Models;
using VelocityAPI.Application.Error;
using VelocityAPI.Application.Database;
using VelocityAPI.Application.Models;
using VelocityAPI.Application.Constants;
using VelocityAPI.Application.DTOs.Auth;
using VelocityAPI.Application.Authentication.Services;
using VelocityAPI.Application.Authentication.Errors;
using VelocityAPI.Application.Authentication.Common;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Net;
using Npgsql;
using Resend;

namespace VelocityAPI.Controllers.Hello;

[ApiController]
[Route("/api/auth")]
public class AuthController : ControllerBase
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        NpgsqlDataSource dataSource,
        IConnectionMultiplexer redis,
        ILogger<AuthController> logger
    )
    {
        _dataSource = dataSource;
        _redis = redis;
        _logger = logger;
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
            _logger.LogError(ex, "Error verifying email");
            return this.RedirectToPage("Something went wrong. Please try again later", 500);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
      [FromBody] LoginRequest request,
      [FromServices] IOptions<AppSettings> settings
    )
    {
        var user = await UserModel.GetUserByEmail(_dataSource, request.Email);
        if (user == null)
        {
            return Error.Unauthorized("Email or password is incorrect");
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!isPasswordValid)
        {
            return Error.Unauthorized("Email or password is incorrect");
        }

        if (!user.EmailVerified)
        {
            return Error.Unauthorized("Please verify your email to login");
        }
        if (user.Strikes == 3)
        {
            return Error.Unauthorized(
  "You have been locked out of the system. This is due to you not buying vehicles after biding on them. Please contact support to unlock your account."
            );
        }

        var refresh = new Refresh(_redis, settings);
        var refreshResponse = await refresh.Create(new TokenParams
        {
            Jti = Ulid.NewUlid().ToString(),
            UserId = user.Id
        });

        var access = new Access(_redis, settings);
        var accessResponse = await access.Create(new TokenParams
        {
            Ajti = refreshResponse.CustomClaim,
            Rjti = refreshResponse.Jti,
            UserId = user.Id,
        });

        var session = new Session(settings);
        var sessionResponse = await session.Create(new TokenParams
        {
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            PhotoUrl = user.PhotoUrl,
        });

        return this.SendTokens(
          refreshResponse,
          accessResponse,
          sessionResponse,
          "Login successful",
          settings
        );
    }

    [HttpPatch("refresh")]
    public async Task<IActionResult> Refresh(
      [FromServices] IOptions<AppSettings> settings
    )
    {
        var refreshToken = HttpContext.Request.Cookies["refresh_token"];
        var sessionToken = HttpContext.Request.Cookies["session_token"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Error.Unauthorized("Refresh token is missing");
        }

        var refresh = new Refresh(_redis, settings);
        var claims = await refresh.Verify(refreshToken);
        if (claims == null)
        {
            throw new TokenValidationException("Invalid refresh token");
        }
        await refresh.Revoke(claims.Jti);

        var refreshResponse = await refresh.Create(new TokenParams
        {
            UserId = claims.UserId,
        });

        var access = new Access(_redis, settings);
        var accessResponse = await access.Create(new TokenParams
        {
            Ajti = refreshResponse.CustomClaim,
            Rjti = refreshResponse.Jti,
            UserId = claims.UserId,
        });

        User user = new User();
        var session = new Session(settings);

        if (!string.IsNullOrEmpty(sessionToken))
        {
            try
            {
                var userDetails = await session.Verify(sessionToken);
                if (
                  userDetails != null &&
                  userDetails.UserId != null &&
                  userDetails.Email != null &&
                  userDetails.Name != null &&
                  userDetails.PhotoUrl != null
                )
                {
                    user.Id = claims.UserId;
                    user.Email = userDetails.Email;
                    user.Name = userDetails.Name;
                    user.PhotoUrl = userDetails.PhotoUrl;
                }
            }
            catch (Exception)
            {
                // ignore
            }
        }
        if (string.IsNullOrEmpty(user.Id))
        {
            var userDetails = await UserModel.GetUserById(_dataSource, claims.UserId);
            if (userDetails == null)
            {
                throw new TokenValidationException("User not found");
            }
            user = userDetails;
        }

        var sessionResponse = await session.Create(new TokenParams
        {
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            PhotoUrl = user.PhotoUrl,
        });

        return this.SendTokens(
          refreshResponse,
          accessResponse,
          sessionResponse,
          "Token refreshed successfully",
          settings
        );
    }

    [HttpDelete("logout")]
    public async Task<IActionResult> Logout(
      [FromServices] IOptions<AppSettings> settings
    )
    {
        var refreshToken = HttpContext.Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(refreshToken))
        {
            return this.DeleteCookies(settings);
        }

        try
        {
            var refresh = new Refresh(_redis, settings);
            await refresh.Revoke(refreshToken);
        }
        catch (Exception)
        {
            // ignore
        }

        return this.DeleteCookies(settings);
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

        _logger.LogInformation("Sent verification email to {Email}", user.Email);
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

    private IActionResult SendTokens(
      TokenResponse refresh,
      TokenResponse access,
      TokenResponse session,
      string message,
      IOptions<AppSettings> settings
    )
    {
        var response = HttpContext.Response;
        response.Cookies.Append("refresh_token", refresh.Token, new CookieOptions
        {
            HttpOnly = true,
            Secure = !Env.IsDevelopment(settings.Value.Environment),
            Domain = settings.Value.Domain,
            Path = "/",
            Expires = refresh.ExpiresAt,
            MaxAge = TimeSpan.FromDays(settings.Value.RefreshTokenExpirationDays),
        });
        response.Cookies.Append("session_token", session.Token, new CookieOptions
        {
            HttpOnly = false,
            Secure = !Env.IsDevelopment(settings.Value.Environment),
            Domain = settings.Value.Domain,
            Path = "/",
            Expires = session.ExpiresAt,
            MaxAge = TimeSpan.FromDays(settings.Value.SessionTokenExpirationDays),
        });
        response.Headers.Append("X-Access-Token", access.Token);

        return Error.Okay(message);
    }

    private IActionResult DeleteCookies(
      IOptions<AppSettings> settings
    )
    {
        var response = HttpContext.Response;
        response.Cookies.Delete("refresh_token", new CookieOptions
        {
            HttpOnly = true,
            Secure = !Env.IsDevelopment(settings.Value.Environment),
            Domain = settings.Value.Domain,
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(-1),
            MaxAge = TimeSpan.FromDays(-1),
        });
        response.Cookies.Delete("session_token", new CookieOptions
        {
            HttpOnly = false,
            Secure = !Env.IsDevelopment(settings.Value.Environment),
            Domain = settings.Value.Domain,
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(-1),
            MaxAge = TimeSpan.FromDays(-1),
        });

        return Error.Okay("Logged out successfully");
    }
}

