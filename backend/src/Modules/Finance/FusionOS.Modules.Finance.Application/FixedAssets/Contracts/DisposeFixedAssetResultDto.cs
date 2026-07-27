using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Contracts;

/// <summary>
/// Result of DisposeFixedAssetCommand — the disposed FixedAsset itself plus
/// the disposal gain/loss figures DisposeFixedAssetCommandHandler computes
/// (AccumulatedDepreciation/NetBookValue as of the disposal date, via
/// FixedAsset.CalculateAccumulatedDepreciation/CalculateBookValue, and
/// GainOrLoss = DisposalProceeds - NetBookValue) and, when the necessary
/// default accounts are configured on FinanceSettings, the real Posted
/// JournalEntry that was written to the GL for it.
///
/// <see cref="JournalEntry"/> is null when GL posting was skipped — same
/// "silently skip rather than fail the disposal" convention
/// DisposeFixedAssetCommandHandler's own doc comment documents: a disposal
/// (a real-world, one-way event) always succeeds and is always recorded even
/// when Finance hasn't configured the accounts needed to post it to the GL.
/// </summary>
public sealed record DisposeFixedAssetResultDto(
    FixedAssetDto FixedAsset,
    decimal AccumulatedDepreciation,
    decimal NetBookValue,
    decimal GainOrLoss,
    JournalEntryDto? JournalEntry);
