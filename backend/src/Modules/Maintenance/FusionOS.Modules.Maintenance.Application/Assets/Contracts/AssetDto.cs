namespace FusionOS.Modules.Maintenance.Application.Assets.Contracts;

public sealed record AssetDto(
    Guid Id,
    string Code,
    string Name,
    string? Location,
    bool IsActive,
    DateTimeOffset? WarrantyExpirationDate,
    string? WarrantyProvider,
    bool IsUnderWarranty,
    DateTimeOffset CreatedAt);

/// <summary>Single place that turns an Asset aggregate into its DTO, shared by every handler that returns one.</summary>
public static class AssetMapper
{
    /// <summary>
    /// IsUnderWarranty is computed here rather than stored — a DTO-level derived field
    /// (WarrantyExpirationDate.HasValue && WarrantyExpirationDate > now), same
    /// "compute at read time, don't persist a derived flag" discipline used for other
    /// point-in-time booleans across the codebase. Kept in the mapper (not a query
    /// filter) so every caller that maps an Asset — Create/Update/GetById/List — gets
    /// a consistent, single-source-of-truth value without duplicating the comparison.
    /// </summary>
    public static AssetDto ToDto(Domain.Assets.Asset asset) =>
        new(
            asset.Id,
            asset.Code,
            asset.Name,
            asset.Location,
            asset.IsActive,
            asset.WarrantyExpirationDate,
            asset.WarrantyProvider,
            asset.WarrantyExpirationDate.HasValue && asset.WarrantyExpirationDate > DateTimeOffset.UtcNow,
            asset.CreatedAt);
}
