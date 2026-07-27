using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Commands.DisposeFixedAsset;

/// <summary>
/// Records that the asset was disposed on a given date, for a given
/// <paramref name="DisposalProceeds"/> (0 if simply discarded/scrapped for
/// nothing), and — as of 2026-07-22 — posts a real, balanced disposal
/// gain/loss JournalEntry to the GL when the necessary default accounts are
/// configured (see DisposeFixedAssetCommandHandler for the calculation and
/// posting logic, and FinanceSettings for the account gating).
/// </summary>
public sealed record DisposeFixedAssetCommand(Guid CompanyId, Guid FixedAssetId, DateTimeOffset DisposedDate, decimal DisposalProceeds)
    : ICommand<DisposeFixedAssetResultDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.fixed-asset.dispose" };
    public string EntityType => nameof(Domain.FixedAssets.FixedAsset);
    public Guid EntityId => FixedAssetId;
    public string Action => "Disposed";
}
