namespace VelocityAPI.Application.Authentication.Common;

public class TokenKeys
{
    public static string getRefreshTokenKey(string jti)
    {
        return $"refresh_token:{jti}";
    }

    public static string getAccessTokenKey(string jti)
    {
        return $"access_token:{jti}";
    }
}
