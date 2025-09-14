namespace VelocityAPI.Models;

public class User
{
    public string Id { get; init; }
    public string Email { get; init; }
    public string Name { get; init; }
    public string PhotoUrl { get; init; }
    public string Nic { get; init; }
    public string PasswordHash { get; init; }
    public int Strikes { get; init; }
    public bool EmailVerified { get; init; }
}
