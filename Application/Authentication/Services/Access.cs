using Microsoft.Extensions.Options;
using StackExchange.Redis;
using VelocityAPI.Application.Authentication.Common;
using VelocityAPI.Application.Authentication.Interfaces;
using VelocityAPI.Application.Authentication.Errors;
using VelocityAPI.Models;

namespace VelocityAPI.Application.Authentication.Services;

public class Access : ITokenService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly AppSettings _settings;

    public Access(IConnectionMultiplexer redis, IOptions<AppSettings> options)
    {
        _redis = redis;
        _settings = options.Value;
    }

    public async Task<TokenResponse> Create(TokenParams tokenParams)
    {
        try
        {
            var rjti = tokenParams.getRjti();
            var isAccessTokenAlreadyGenerated = !string.IsNullOrEmpty(tokenParams.Ajti);
            var ajti = isAccessTokenAlreadyGenerated ? tokenParams.getAjti() : Ulid.NewUlid().ToString();

            var claims = new TokenClaims
            {
                Jti = ajti,
                UserId = tokenParams.UserId,
            };

            var response = JwtTokenGenerator.Generate(claims, _settings.JWTSecret, _settings.AccessTokenExpirationMinutes);
            if (response == null)
            {
                throw new Exception("An error occurred while creating the access token.");
            }

            if (!isAccessTokenAlreadyGenerated)
            {
                var db = _redis.GetDatabase();
                await db.StringSetAsync(
                  TokenKeys.getAccessTokenKey(ajti),
                  tokenParams.UserId,
                  TimeSpan.FromMinutes(_settings.AccessTokenExpirationMinutes)
                );
            }
            return response;
        }
        catch (Exception ex)
        {
            throw new TokenInternalException(ex);
        }
    }

    public async Task<TokenClaims> Verify(string token)
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
            var refreshTokenJti = claims.FirstOrDefault(c => c.Type == "rjti")?.Value;

            var missing = new List<string>();
            if (string.IsNullOrEmpty(jti)) missing.Add("jti");
            if (string.IsNullOrEmpty(userId)) missing.Add("sub");
            if (string.IsNullOrEmpty(refreshTokenJti)) missing.Add("rjti");

            if (missing.Count > 0)
            {
                throw new TokenValidationException($"Token is missing the following claims: {string.Join(", ", missing)}");
            }

            var db = _redis.GetDatabase();

            var userIdFromRedis = await db.StringGetAsync(
              TokenKeys.getAccessTokenKey(jti!)
            );

            if (string.IsNullOrEmpty(userIdFromRedis) || userIdFromRedis != userId)
            {
                throw new TokenValidationException("Access token is invalid or has expired.");
            }

            return new TokenClaims
            {
                Jti = jti!,
                UserId = userId!,
                Rjti = refreshTokenJti!
            };
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
