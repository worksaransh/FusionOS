using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Commands.DeactivateFixedAsset;

/// <summary>Soft-deactivate only — never a real delete (04_DATABASE_GUIDELINES.md), same convention as every other M8 sub-slice's Deactivate command. Independent of Dispose — see FixedAsset.Deactivate's own doc comment for the distinction.</summary>
public sealed record DeactivateFixedAssetCommand(Guid CompanyId, Guid FixedAssetId)
    : ICommand<FixedAssetDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.fixed-asset.deactivate" };
    public string EntityType => nameof(Domain.FixedAssets.FixedAsset);
    public Guid EntityId => FixedAssetId;
    public string Action => "Deactivated";
}
