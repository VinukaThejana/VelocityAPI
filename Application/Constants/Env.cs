namespace VelocityAPI.Application.Constants;

public enum EnvironmentType
{
    Development,
    Production,
    Staging
}

public class Env
{
    public EnvironmentType Production { get; set; } = EnvironmentType.Production;
    public EnvironmentType Development { get; set; } = EnvironmentType.Development;
    public EnvironmentType Staging { get; set; } = EnvironmentType.Staging;

    public static EnvironmentType Parse(string environment)
    {
        return environment.ToLower().Trim() switch
        {
            "prd" or "production" or "prod" => EnvironmentType.Production,
            "dev" or "development" => EnvironmentType.Development,
            "stg" or "staging" => EnvironmentType.Staging,
            _ => throw new ArgumentException("Invalid environment type"),
        };
    }

    public static bool IsProduction(string environment) => Parse(environment) == EnvironmentType.Production;
    public static bool IsDevelopment(string environment) => Parse(environment) == EnvironmentType.Development;
    public static bool IsStaging(string environment) => Parse(environment) == EnvironmentType.Staging;

    public static string ToString(EnvironmentType environment)
    {
        return environment switch
        {
            EnvironmentType.Production => "production",
            EnvironmentType.Development => "development",
            EnvironmentType.Staging => "staging",
            _ => "unknown",
        };
    }
}
