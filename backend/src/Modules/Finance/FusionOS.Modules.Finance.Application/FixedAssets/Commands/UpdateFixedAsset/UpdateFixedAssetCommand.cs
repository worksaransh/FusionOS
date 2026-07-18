using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Commands.UpdateFixedAsset;

/// <summary>Name/CostCenterId only — see FixedAsset.UpdateDetails's own doc comment for why AcquisitionCost/SalvageValue/UsefulLifeMonths/AssetAccountId/AccumulatedDepreciationAccountId are not editable here.</summary>
public sealed record UpdateFixedAssetCommand(Guid CompanyId, Guid FixedAssetId, string Name, Guid? CostCenterId)
    : ICommand<FixedAssetDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.fixed-asset.update" };
    public string EntityType => nameof(Domain.FixedAssets.FixedAsset);
    public Guid EntityId => FixedAssetId;
    public string Action => "Updated";
}
