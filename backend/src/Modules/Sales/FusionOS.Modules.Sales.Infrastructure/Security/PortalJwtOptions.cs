namespace FusionOS.Modules.Sales.Infrastructure.Security;

/// <summary>
/// Bound from the SAME "Jwt" configuration section Core.Infrastructure.Security.JwtOptions
/// binds from (see appsettings.json/Jwt:SigningKey) - deliberately not a
/// reference to Core's own JwtOptions class (Sales cannot reference
/// Core.Infrastructure - see IPortalTokenService's doc comment), just a
/// second, independent class reading the identical section so both Core's and
/// Sales' JWT issuers sign with the same key/issuer/audience and the single
/// JWT bearer scheme in Program.cs can validate tokens from either.
/// </summary>
public sealed class PortalJwtOptions
{
    public const string SectionName = "Jwt";

    public string SigningKey { get; set; } = default!;
    public string Issuer { get; set; } = "fusionos";
    public string Audience { get; set; } = "fusionos-clients";

    /// <summary>
    /// Deliberately shorter-lived than staff tokens (Core's default is 15) and
    /// its own setting rather than reusing AccessTokenLifetimeMinutes from the
    /// same section, since a customer portal session is a smaller trust
    /// surface than an internal staff session - only ever used if
    /// "Jwt:PortalAccessTokenLifetimeMinutes" is present, defaults to 15 to
    /// match Core's own default otherwise.
    /// </summary>
    public int PortalAccessTokenLifetimeMinutes { get; set; } = 15;
}
