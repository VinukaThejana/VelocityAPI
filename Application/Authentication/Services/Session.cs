using Microsoft.Extensions.Options;
using System.Security.Claims;
using VelocityAPI.Application.Authentication.Common;
using VelocityAPI.Application.Authentication.Interfaces;
using VelocityAPI.Application.Authentication.Errors;
using VelocityAPI.Models;

namespace VelocityAPI.Application.Authentication.Services;

public class Session : ITokenService
{
    private readonly AppSettings _settings;

    public Session(IOptions<AppSettings> options)
    {
        _settings = options.Value;
    }

    public Task<TokenResponse> Create(TokenParams tokenParams)
    {
        var claims = new TokenClaims
        {
            Jti = Ulid.NewUlid().ToString(),
            UserId = tokenParams.UserId,
            Name = tokenParams.Name,
            Email = tokenParams.Email,
            PhotoUrl = tokenParams.PhotoUrl,
        };

        var response = JwtTokenGenerator.Generate(claims, _settings.JWTSecret, _settings.SessionTokenExpirationDays * 24 * 60);
        return Task.FromResult(response);
    }

    public Task<TokenClaims> Verify(string token)
    {
        var principal = JwtTokenGenerator.GetPrincipalFromToken(token, _settings.JWTSecret);
        if (principal == null)
        {
            throw new TokenValidationException("Invalid token.");
        }

        var claims = principal.Claims;

        var jti = claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;
        var userId = claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var photoUrl = claims.FirstOrDefault(c => c.Type == "photoUrl")?.Value;

        var missing = new List<string>();
        if (string.IsNullOrEmpty(jti)) missing.Add("jti");
        if (string.IsNullOrEmpty(userId)) missing.Add("sub");
        if (string.IsNullOrEmpty(name)) missing.Add("name");
        if (string.IsNullOrEmpty(email)) missing.Add("email");
        if (string.IsNullOrEmpty(photoUrl)) missing.Add("photoUrl");

        if (missing.Count > 0)
        {
            throw new TokenValidationException($"Token is missing the following claims: {string.Join(", ", missing)}");
        }

        return Task.FromResult(new TokenClaims
        {
            Jti = jti!,
            UserId = userId!,
            Name = name!,
            Email = email!,
            PhotoUrl = photoUrl!
        });
    }
}


