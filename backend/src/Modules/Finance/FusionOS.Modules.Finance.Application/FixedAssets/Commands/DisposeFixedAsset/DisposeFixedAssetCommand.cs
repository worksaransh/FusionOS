using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Commands.DisposeFixedAsset;

/// <summary>Records that the asset was disposed on a given date — no gain/loss-on-disposal calculation or GL posting (see FixedAsset.Dispose's own doc comment).</summary>
public sealed record DisposeFixedAssetCommand(Guid CompanyId, Guid FixedAssetId, DateTimeOffset DisposedDate)
    : ICommand<FixedAssetDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.fixed-asset.dispose" };
    public string EntityType => nameof(Domain.FixedAssets.FixedAsset);
    public Guid EntityId => FixedAssetId;
    public string Action => "Disposed";
}
