using System.Security.Claims;

namespace VelocityAPI.Application.Authentication.Common;

public class TokenClaims
{
    public required string UserId { get; init; }
    public required string Jti { get; init; }
    public string? Rjti { get; init; }

    public string? Name { get; init; }
    public string? Email { get; init; }
    public string? PhotoUrl { get; init; }
    public string? Custom { get; init; }

    public List<Claim> ToClaimList()
    {
        var claims = new List<Claim>
      {
        new(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, UserId),
        new(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, Jti),
      };

        if (!string.IsNullOrEmpty(Rjti))
        {
            claims.Add(new("rjti", Rjti));
        }

        if (!string.IsNullOrEmpty(Name))
        {
            claims.Add(new(ClaimTypes.Name, Name));
        }

        if (!string.IsNullOrEmpty(Email))
        {
            claims.Add(new(ClaimTypes.Email, Email));
        }

        if (!string.IsNullOrEmpty(PhotoUrl))
        {
            claims.Add(new("photo_url", PhotoUrl));
        }

        if (!string.IsNullOrEmpty(Custom))
        {
            claims.Add(new("custom", Custom));
        }

        return claims;
    }
}
