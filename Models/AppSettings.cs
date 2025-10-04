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
        public string JWTSecret { get; set; } = string.Empty;

        [Required]
        public string ResendAPIKey { get; set; } = string.Empty;

        [Required]
        public string AwsAccessKeyId { get; set; } = string.Empty;

        [Required]
        public string AwsAccessKeySecret { get; set; } = string.Empty;

        [Required]
        public string AwsRegion { get; set; } = string.Empty;

        [Required]
        public string AwsBucket { get; set; } = string.Empty;

        [Required]
        public string RouteSecret { get; set; } = string.Empty;

        [Required]
        public string Environment { get; set; } = string.Empty;

        [Required]
        public string Domain { get; set; } = string.Empty;

        [Required]
        [Range(1, int.MaxValue)]
        public int AccessTokenExpirationMinutes { get; set; } = 60;

        [Required]
        [Range(1, int.MaxValue)]
        public int RefreshTokenExpirationDays { get; set; } = 7;

        [Required]
        [Range(1, int.MaxValue)]
        public int SessionTokenExpirationDays { get; set; } = 7;

        [Required]
        public string DbURL { get; set; } = string.Empty;
    }
}
