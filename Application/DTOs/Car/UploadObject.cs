using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace VelocityAPI.Application.DTOs.Car;

public class URL
{
    [JsonPropertyName("url")]
    [Required(ErrorMessage = "The url field is required.")]
    public string Url { get; set; } = string.Empty;

    [JsonPropertyName("key")]
    [Required(ErrorMessage = "The key field is required.")]
    public string Key { get; set; } = string.Empty;
}
