using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using BCrypt.Net;

namespace VelocityAPI.DTOs.Auth;

public class RegisterRequest
{
    [JsonPropertyName("email")]
    [Required(ErrorMessage = "The email field is required.")]
    [EmailAddress(ErrorMessage = "The email field is not a valid email address.")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    [Required(ErrorMessage = "The name field is required.")]
    [MinLength(2, ErrorMessage = "The name field must be at least 2 characters long.")]
    [MaxLength(50, ErrorMessage = "The name field must be at most 50 characters long.")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("nic")]
    [Required(ErrorMessage = "The nic field is required.")]
    [MinLength(10, ErrorMessage = "The nic field must be at least 10 characters long.")]
    [MaxLength(20, ErrorMessage = "The nic field must be at most 20 characters long.")]
    public string Nic { get; set; } = string.Empty;

    [JsonPropertyName("password")]
    [Required(ErrorMessage = "The password field is required.")]
    public string Password { get; set; } = string.Empty;

    public string getPhotoUrl()
    {
        return $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(Name)}&background=random";
    }

    public string getHashedPassword()
    {
        return BCrypt.Net.BCrypt.HashPassword(Password);
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrEmpty(Password))
        {
            // Rule 1: At least 6 characters long
            if (Password.Length < 6)
            {
                yield return new ValidationResult(
                  "The password field must be at least 6 characters long.",
                  new[] { nameof(Password) }
                );
            }

            // Rule 2: Contains at least one number
            if (!Regex.IsMatch(Password, @"[0-9]"))
            {
                yield return new ValidationResult(
                  "The password field must contain at least one number.",
                  new[] { nameof(Password) }
                );
            }

            // Rule 3: Contains at least one uppercase letter
            if (!Regex.IsMatch(Password, @"[A-Z]"))
            {
                yield return new ValidationResult(
                  "The password field must contain at least one uppercase letter.",
                  new[] { nameof(Password) }
                );
            }

            // Rule 4: Contains at least one symbol
            if (!Regex.IsMatch(Password, @"[^a-zA-Z0-9]"))
            {
                yield return new ValidationResult(
                  "The password field must contain at least one symbol.",
                  new[] { nameof(Password) }
                );
            }
        }
    }
}
