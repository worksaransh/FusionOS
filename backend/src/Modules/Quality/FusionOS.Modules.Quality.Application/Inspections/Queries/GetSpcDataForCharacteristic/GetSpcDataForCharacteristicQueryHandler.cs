using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Inspections.Queries.GetSpcDataForCharacteristic;

public sealed class GetSpcDataForCharacteristicQueryHandler : IRequestHandler<GetSpcDataForCharacteristicQuery, SpcDataForCharacteristicDto>
{
    private const string ScopeNote =
        "Raw SPC data only: this returns every recorded MeasuredValue for this characteristic name plus the most-recently-set " +
        "control limits found — it does not compute Cp/Cpk process-capability indices, and there is no per-characteristic-spec " +
        "master record reconciling control limits set differently across different inspections of the same characteristic name " +
        "(the limits shown are whichever matching inspection item was created most recently). See " +
        "GetSpcDataForCharacteristicQuery's doc comment for the full scope decision.";

    private readonly IInspectionRepository _repository;

    public GetSpcDataForCharacteristicQueryHandler(IInspectionRepository repository) => _repository = repository;

    public async Task<SpcDataForCharacteristicDto> Handle(GetSpcDataForCharacteristicQuery request, CancellationToken cancellationToken)
    {
        var inspections = await _repository.ListByCharacteristicAsync(
            request.CompanyId, request.CharacteristicName, request.PeriodStart, request.PeriodEnd, cancellationToken);

        var dataPoints = new List<SpcDataPointDto>();
        decimal? lowerControlLimit = null;
        decimal? upperControlLimit = null;
        DateTimeOffset? latestItemAt = null;

        foreach (var inspection in inspections)
        {
            foreach (var item in inspection.Items)
            {
                if (!string.Equals(item.Characteristic, request.CharacteristicName, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!item.MeasuredValue.HasValue)
                    continue;

                dataPoints.Add(new SpcDataPointDto(inspection.Id, inspection.CreatedAt, item.MeasuredValue.Value));

                // "Most recently found" control limits — see the query's doc comment for why
                // this doesn't attempt to reconcile inconsistent limits across inspections.
                if (latestItemAt is null || inspection.CreatedAt >= latestItemAt.Value)
                {
                    latestItemAt = inspection.CreatedAt;
                    lowerControlLimit = item.LowerControlLimit;
                    upperControlLimit = item.UpperControlLimit;
                }
            }
        }

        return new SpcDataForCharacteristicDto(
            request.CharacteristicName,
            request.PeriodStart,
            request.PeriodEnd,
            dataPoints,
            lowerControlLimit,
            upperControlLimit,
            ScopeNote);
    }
}
