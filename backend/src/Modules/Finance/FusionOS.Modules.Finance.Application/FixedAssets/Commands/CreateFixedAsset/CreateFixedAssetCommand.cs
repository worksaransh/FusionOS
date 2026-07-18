using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Commands.CreateFixedAsset;

public sealed record CreateFixedAssetCommand(
    Guid CompanyId,
    string Code,
    string Name,
    Guid AssetAccountId,
    Guid? AccumulatedDepreciationAccountId,
    Guid? CostCenterId,
    DateTimeOffset AcquisitionDate,
    decimal AcquisitionCost,
    decimal SalvageValue,
    int UsefulLifeMonths)
    : ICommand<FixedAssetDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.fixed-asset.create" };
    public string EntityType => nameof(Domain.FixedAssets.FixedAsset);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
