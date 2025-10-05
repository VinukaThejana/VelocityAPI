using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VelocityAPI.Application.DTOs.Car;

public class UploadRequest
{
    [JsonPropertyName("filenames")]
    [Required(ErrorMessage = "The filenames field is required.")]
    public List<string> FileNames { get; set; } = new List<string>();
}
