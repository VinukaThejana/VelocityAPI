namespace VelocityAPI.Application.Authentication.Errors;

public class TokenInternalException : Exception
{
    public TokenInternalException(string message) : base(message)
    {

    }

    public TokenInternalException(string message, Exception innerException) : base(message, innerException)
    {

    }
}
