namespace VelocityAPI.Models;

public class User
{
    public string Id { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string PhotoUrl { get; init; } = string.Empty;

    public string Nic { get; init; } = string.Empty;

    public string PasswordHash { get; init; } = string.Empty;

    public int Strikes { get; init; } = 0;

    public bool EmailVerified { get; init; } = false;
}
