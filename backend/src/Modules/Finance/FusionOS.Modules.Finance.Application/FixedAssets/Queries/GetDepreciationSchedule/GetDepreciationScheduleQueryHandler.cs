using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Queries.GetDepreciationSchedule;

/// <summary>
/// Computes straight-line depreciation for a single FixedAsset as of a given
/// date, entirely on demand from that asset's own AcquisitionDate/
/// AcquisitionCost/SalvageValue/UsefulLifeMonths fields.
///
/// <b>Nothing here is persisted.</b> There is no DepreciationScheduleEntry
/// table, no row written per month, and no JournalEntry ever created by this
/// query — this handler only computes and returns numbers. Posting
/// depreciation to the GL (a recurring monthly process that would create
/// real, Posted JournalEntry records debiting a depreciation-expense account
/// and crediting AccumulatedDepreciationAccountId) is an explicit, distinct,
/// separately-scoped future follow-up — see FixedAsset.cs's own class doc
/// comment for the same scope line stated from the aggregate's side.
///
/// <b>Monthly depreciation amount</b> = (AcquisitionCost - SalvageValue) /
/// UsefulLifeMonths — the constant straight-line figure, the same every
/// month for the asset's whole useful life.
///
/// <b>Months elapsed</b> is the count of whole calendar months between
/// AcquisitionDate and AsOfDate (a month only counts once AsOfDate's
/// day-of-month has reached AcquisitionDate's day-of-month — the same
/// "whole months only" idea a loan amortization schedule uses), clamped to
/// the range [0, UsefulLifeMonths]: an AsOfDate before AcquisitionDate
/// yields zero months elapsed (nothing has depreciated yet) rather than a
/// negative count, and an AsOfDate far past the asset's useful life still
/// caps at UsefulLifeMonths — depreciation stops once the asset is fully
/// depreciated, it never exceeds the depreciable base
/// (AcquisitionCost - SalvageValue).
///
/// <b>AccumulatedDepreciation</b> = MonthlyDepreciationAmount * MonthsElapsed,
/// and <b>BookValue</b> = AcquisitionCost - AccumulatedDepreciation.
///
/// <b>2026-07-22:</b> the actual calculation now lives on FixedAsset itself
/// (CalculateMonthlyDepreciationAmount/CalculateMonthsElapsed/
/// CalculateAccumulatedDepreciation/CalculateBookValue) so that
/// DisposeFixedAssetCommandHandler's disposal gain/loss posting can reuse the
/// exact same figures rather than re-deriving them independently. This
/// handler is now a thin read-only wrapper around those methods; the
/// numbers/behavior are unchanged.
/// </summary>
public sealed class GetDepreciationScheduleQueryHandler : IRequestHandler<GetDepreciationScheduleQuery, DepreciationScheduleDto>
{
    private readonly IFixedAssetRepository _repository;

    public GetDepreciationScheduleQueryHandler(IFixedAssetRepository repository) => _repository = repository;

    public async Task<DepreciationScheduleDto> Handle(GetDepreciationScheduleQuery request, CancellationToken cancellationToken)
    {
        var fixedAsset = await _repository.GetByIdAsync(request.CompanyId, request.FixedAssetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Fixed asset '{request.FixedAssetId}' was not found.");

        var monthlyDepreciationAmount = fixedAsset.CalculateMonthlyDepreciationAmount();
        var monthsElapsed = fixedAsset.CalculateMonthsElapsed(request.AsOfDate);
        var accumulatedDepreciation = fixedAsset.CalculateAccumulatedDepreciation(request.AsOfDate);
        var bookValue = fixedAsset.CalculateBookValue(request.AsOfDate);

        return new DepreciationScheduleDto(fixedAsset.Id, monthlyDepreciationAmount, monthsElapsed, accumulatedDepreciation, bookValue);
    }
}
