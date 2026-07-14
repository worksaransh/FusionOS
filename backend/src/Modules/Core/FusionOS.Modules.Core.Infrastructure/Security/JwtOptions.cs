namespace FusionOS.Modules.Core.Infrastructure.Security;

/// <summary>Bound from the "Jwt" configuration section - see appsettings.json and 07_SECURITY.md.</summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; set; } = default!;
    public string Issuer { get; set; } = "fusionos";
    public string Audience { get; set; } = "fusionos-clients";
    public int AccessTokenLifetimeMinutes { get; set; } = 15;
    public int RefreshTokenLifetimeDays { get; set; } = 30;
}
