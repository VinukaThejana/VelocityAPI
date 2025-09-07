using System.ComponentModel.DataAnnotations;

namespace VelocityAPI.Models
{
    public class AppSettings
    {
        [Required(AllowEmptyStrings = true)]
        public string Port { get; set; } = string.Empty;

        [Required]
        public string RedisURL { get; set; } = string.Empty;

        [Required]
        public string DbURL { get; set; } = string.Empty;
    }
}
