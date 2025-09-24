using Microsoft.Extensions.Options;
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
        try
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
            if (response == null)
            {
                throw new Exception("An error occurred while generating the session token");
            }
            return Task.FromResult(response);
        }
        catch (Exception ex)
        {
            throw new TokenInternalException(ex);
        }
    }

    public Task<TokenClaims> Verify(string token)
    {
        try
        {
            var principal = JwtTokenGenerator.GetPrincipalFromToken(token, _settings.JWTSecret);
            if (principal == null)
            {
                throw new TokenValidationException("Invalid token.");
            }

            var claims = principal.Claims;

            var jti = claims.FirstOrDefault(c => c.Type == "jti")?.Value;
            var userId = claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var name = claims.FirstOrDefault(c => c.Type == "unique_name")?.Value;
            var email = claims.FirstOrDefault(c => c.Type == "email")?.Value;
            var photoUrl = claims.FirstOrDefault(c => c.Type == "photo_url")?.Value;

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
        catch (TokenValidationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new TokenInternalException(ex);
        }
    }
}


