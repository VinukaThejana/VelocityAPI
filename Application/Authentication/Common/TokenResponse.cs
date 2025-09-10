namespace VelocityAPI.Application.Authentication.Common;

public class TokenResponse
{
    public required string Token { get; init; }
    public required DateTime ExpiresAt { get; init; }
}
