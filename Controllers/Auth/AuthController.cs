using Microsoft.AspNetCore.Mvc;
using Npgsql;
using VelocityAPI.DTOs.Auth;
using VelocityAPI.Application.Error;
using VelocityAPI.Application.Database;

namespace VelocityAPI.Controllers.Hello;

[ApiController]
[Route("/api/auth")]
public class AuthController : ControllerBase
{
    private readonly NpgsqlDataSource _dataSource;

    public AuthController(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Regsiter([FromBody] RegisterRequest request)
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

            // NOTE: Resend verification email logic here
            return Error.Okay("Verification email resent");
        }

        var newUser = await UserModel.CreateUser(_dataSource, request);

        // NOTE: Send verification email logic here
        return Error.Okay("Verification email sent");
    }
}

