using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.Assets.Contracts;

namespace FusionOS.Modules.Maintenance.Application.Assets.Commands.UpdateAsset;

/// <summary>Update deliberately excludes Code — same "the business key is set at creation" convention as UpdateMaintenanceScheduleCommand excluding AssetId.</summary>
public sealed record UpdateAssetCommand(Guid CompanyId, Guid AssetId, string Name, string? Location, DateTimeOffset? WarrantyExpirationDate, string? WarrantyProvider)
    : ICommand<AssetDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "maintenance.asset.update" };
    public string EntityType => nameof(Domain.Assets.Asset);
    public Guid EntityId => AssetId;
    public string Action => "Updated";
}
