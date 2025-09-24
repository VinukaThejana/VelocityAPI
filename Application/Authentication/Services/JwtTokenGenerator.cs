using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VelocityAPI.Application.Authentication.Common;
using VelocityAPI.Application.Authentication.Errors;

namespace VelocityAPI.Application.Authentication.Services;

public class JwtTokenGenerator
{
    public static TokenResponse Generate(TokenClaims claims, string secretKey, int expiresInMinutes)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secretKey);
            var expires = DateTime.UtcNow.AddMinutes(expiresInMinutes);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims.ToClaimList()),
                Expires = expires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                NotBefore = DateTime.UtcNow,
                IssuedAt = DateTime.UtcNow,
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return new TokenResponse
            {
                Jti = claims.Jti,
                Token = tokenString,
                ExpiresAt = expires
            };
        }
        catch (Exception ex)
        {
            throw new TokenInternalException(ex);
        }
    }

    public static ClaimsPrincipal GetPrincipalFromToken(string token, string secretKey)
    {
        try
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            tokenHandler.InboundClaimTypeMap.Clear();


            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token algorithm");
            }

            return principal;
        }
        catch (Exception ex)
        {
            throw new TokenValidationException("Failed to validate token", ex);
        }
    }

    public bool IsTokenValid(string token, string secretKey)
    {
        try
        {
            GetPrincipalFromToken(token, secretKey);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
