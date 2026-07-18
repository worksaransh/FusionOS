namespace FusionOS.Modules.Finance.Application.FixedAssets.Contracts;

/// <summary>
/// Result of GetDepreciationScheduleQuery — see that query's handler for the
/// straight-line calculation this is built from. Nothing this DTO carries is
/// persisted anywhere: it is recomputed from the FixedAsset's own fields on
/// every request.
/// </summary>
public sealed record DepreciationScheduleDto(
    Guid FixedAssetId,
    decimal MonthlyDepreciationAmount,
    int MonthsElapsed,
    decimal AccumulatedDepreciation,
    decimal BookValue);
