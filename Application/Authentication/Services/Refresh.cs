using Microsoft.Extensions.Options;
using StackExchange.Redis;
using VelocityAPI.Application.Authentication.Common;
using VelocityAPI.Application.Authentication.Interfaces;
using VelocityAPI.Application.Authentication.Errors;
using VelocityAPI.Models;

namespace VelocityAPI.Application.Authentication.Services;

public class Refresh : ITokenService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly AppSettings _settings;

    public Refresh(IConnectionMultiplexer redis, IOptions<AppSettings> options)
    {
        _redis = redis;
        _settings = options.Value;
    }

    public async Task<TokenResponse> Create(TokenParams tokenParams)
    {
        try
        {
            var rjti = Ulid.NewUlid().ToString();
            var ajti = Ulid.NewUlid().ToString();

            var claims = new TokenClaims
            {
                Jti = rjti,
                UserId = tokenParams.UserId,
                Custom = ajti
            };

            var response = JwtTokenGenerator.Generate(claims, _settings.JWTSecret, _settings.RefreshTokenExpirationDays * 24 * 60);
            response.CustomClaim = ajti;

            var db = _redis.GetDatabase();
            var batch = db.CreateBatch();

            var setRefreshTask = batch.StringSetAsync(
              TokenKeys.getRefreshTokenKey(rjti),
              ajti,
              TimeSpan.FromDays(_settings.RefreshTokenExpirationDays)
            );

            var setAccessTask = batch.StringSetAsync(
              TokenKeys.getAccessTokenKey(ajti),
              tokenParams.UserId,
              TimeSpan.FromMinutes(_settings.AccessTokenExpirationMinutes)
            );

            batch.Execute();
            await Task.WhenAll(setRefreshTask, setAccessTask);

            return response;
        }
        catch (Exception ex)
        {
            throw new TokenInternalException("An error occurred while creating the refresh token.", ex);
        }
    }

    public async Task<TokenClaims> Verify(string token)
    {
        var principal = JwtTokenGenerator.GetPrincipalFromToken(token, _settings.JWTSecret);
        if (principal == null)
        {
            throw new TokenValidationException("Invalid token.");
        }

        var claims = principal.Claims;

        var jti = claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;
        var userId = claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
        var custom = claims.FirstOrDefault(c => c.Type == "custom")?.Value;

        var missing = new List<string>();
        if (string.IsNullOrEmpty(jti)) missing.Add("jti");
        if (string.IsNullOrEmpty(userId)) missing.Add("sub");
        if (string.IsNullOrEmpty(custom)) missing.Add("custom");

        if (missing.Count > 0)
        {
            throw new TokenValidationException($"Token is missing the following claims: {string.Join(", ", missing)}");
        }

        var db = _redis.GetDatabase();

        try
        {
            var accessTokenJti = await db.StringGetAsync(
              TokenKeys.getRefreshTokenKey(jti!)
            );

            if (accessTokenJti.IsNullOrEmpty || accessTokenJti != custom)
            {
                throw new TokenValidationException("Refresh token is invalid or has been revoked.");
            }
        }
        catch (Exception ex)
        {
            throw new TokenInternalException("An error occurred while verifying the refresh token.", ex);
        }

        return new TokenClaims
        {
            Jti = jti!,
            UserId = userId!,
            Custom = custom!,
        };
    }

    public async Task Revoke(string tokenOrRefreshTokenJti)
    {
        var jti = tokenOrRefreshTokenJti;

        if (jti.Length != 26) // Not  a Ulid
        {
            var principal = JwtTokenGenerator.GetPrincipalFromToken(tokenOrRefreshTokenJti, _settings.JWTSecret);
            if (principal == null)
            {
                throw new TokenValidationException("Invalid token.");
            }

            var refreshTokenJti = principal.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrEmpty(refreshTokenJti))
            {
                throw new TokenValidationException("Token is missing the jti claim.");
            }

            jti = refreshTokenJti;
        }

        try
        {
            var db = _redis.GetDatabase();

            var accessTokenJti = await db.StringGetAsync(
              TokenKeys.getRefreshTokenKey(jti)
            );
            if (string.IsNullOrEmpty(accessTokenJti))
            {
                throw new TokenValidationException("Refresh token is invalid or has already been revoked.");
            }

            var batch = db.CreateBatch();

            await batch.KeyDeleteAsync(
              TokenKeys.getRefreshTokenKey(jti)
            );
            await batch.KeyDeleteAsync(
              TokenKeys.getAccessTokenKey(accessTokenJti!)
            );

            batch.Execute();
        }
        catch (Exception ex)
        {
            throw new TokenInternalException("An error occurred while revoking the refresh token.", ex);
        }
    }
}
