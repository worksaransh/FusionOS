namespace FusionOS.Modules.Maintenance.Application.Assets.Contracts;

public sealed record AssetDto(Guid Id, string Code, string Name, string? Location, bool IsActive, DateTimeOffset CreatedAt);

/// <summary>Single place that turns an Asset aggregate into its DTO, shared by every handler that returns one.</summary>
public static class AssetMapper
{
    public static AssetDto ToDto(Domain.Assets.Asset asset) =>
        new(asset.Id, asset.Code, asset.Name, asset.Location, asset.IsActive, asset.CreatedAt);
}
