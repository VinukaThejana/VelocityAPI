namespace VelocityAPI.Application.Authentication.Common;

public class TokenResponse
{
    public required string Jti { get; init; }
    public required string Token { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public string CustomClaim { get; set; }
}
