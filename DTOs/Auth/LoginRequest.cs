using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VelocityAPI.DTOs.Auth;

public class LoginRequest
{
    [JsonPropertyName("email")]
    [Required(ErrorMessage = "The email field is required.")]
    [EmailAddress(ErrorMessage = "The email field is not a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    [Required(ErrorMessage = "The password field is required.")]
    public string Password { get; set; } = string.Empty;
}
