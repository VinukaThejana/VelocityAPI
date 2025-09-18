namespace VelocityAPI.Models;

public class User
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string PhotoUrl { get; set; } = string.Empty;

    public string Nic { get; init; } = string.Empty;

    public string PasswordHash { get; init; } = string.Empty;

    public int Strikes { get; init; } = 0;

    public bool EmailVerified { get; init; } = false;
}
