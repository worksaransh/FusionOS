namespace FusionOS.Modules.Finance.Application.FixedAssets.Contracts;

public sealed record FixedAssetDto(
    Guid Id,
    string Code,
    string Name,
    Guid AssetAccountId,
    Guid? AccumulatedDepreciationAccountId,
    Guid? CostCenterId,
    DateTimeOffset AcquisitionDate,
    decimal AcquisitionCost,
    decimal SalvageValue,
    int UsefulLifeMonths,
    bool IsDisposed,
    DateTimeOffset? DisposedDate,
    bool IsActive,
    DateTimeOffset CreatedAt);
