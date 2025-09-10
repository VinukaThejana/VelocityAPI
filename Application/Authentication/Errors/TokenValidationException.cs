namespace VelocityAPI.Application.Authentication.Errors;

public class TokenValidationException : Exception
{
    public TokenValidationException(string message) : base(message)
    {

    }

    public TokenValidationException(string message, Exception innerException) : base(message, innerException)
    {

    }
}
