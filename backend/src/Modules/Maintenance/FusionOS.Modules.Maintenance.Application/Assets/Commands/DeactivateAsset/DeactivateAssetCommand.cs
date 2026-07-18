using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.Assets.Contracts;

namespace FusionOS.Modules.Maintenance.Application.Assets.Commands.DeactivateAsset;

public sealed record DeactivateAssetCommand(Guid CompanyId, Guid AssetId)
    : ICommand<AssetDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "maintenance.asset.deactivate" };
    public string EntityType => nameof(Domain.Assets.Asset);
    public Guid EntityId => AssetId;
    public string Action => "Deactivated";
}
