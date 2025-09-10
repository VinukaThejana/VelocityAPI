using VelocityAPI.Application.Authentication.Common;

namespace VelocityAPI.Application.Authentication.Interfaces;

public interface ITokenService
{
    Task<TokenResponse> Create(TokenParams tokenParams);

    Task<TokenClaims> Verify(string token);
}
