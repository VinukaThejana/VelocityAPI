namespace VelocityAPI.Application.Authentication.Common;

public class TokenParams
{
    public required string UserId { get; init; }

    public string Jti { get; init; } = string.Empty;
    public string Ajti { get; init; } = string.Empty;
    public string Rjti { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string PhotoUrl { get; init; } = string.Empty;

    public string getAjti()
    {
        if (string.IsNullOrEmpty(Ajti))
        {
            throw new ArgumentNullException("ajti is null or empty");
        }
        return Ajti;
    }

    public string getRjti()
    {
        if (string.IsNullOrEmpty(Rjti))
        {
            throw new ArgumentNullException("rjti is null or empty");
        }
        return Rjti;
    }

    public string getName()
    {
        if (string.IsNullOrEmpty(Name))
        {
            throw new ArgumentNullException("name is null or empty");
        }
        return Name;
    }

    public string getEmail()
    {
        if (string.IsNullOrEmpty(Email))
        {
            throw new ArgumentNullException("email is null or empty");
        }
        return Email;
    }

    public string getPhotoUrl()
    {
        if (string.IsNullOrEmpty(PhotoUrl))
        {
            throw new ArgumentNullException("photoUrl is null or empty");
        }
        return PhotoUrl;
    }
}

