using FusionOS.Modules.BusinessIntelligence.Application.KpiDefinitions.Contracts;
using FusionOS.Modules.BusinessIntelligence.Application.KpiValues.Contracts;
using MediatR;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiValues.Queries.GetLiveKpiValue;

/// <summary>
/// Phase 2.3 — resolves the KpiDefinition, finds the IKpiDataSource whose KpiCode
/// matches it, and asks that data source for the current value straight from the
/// owning module's existing report query. No stored snapshot is read or required;
/// bi.kpi-snapshot.record remains available for KPIs that have no live data source
/// (manual/qualitative KPIs), but the three wired here (AR_OUTSTANDING,
/// AP_OUTSTANDING, LOW_STOCK_COUNT) no longer need one.
/// </summary>
public sealed class GetLiveKpiValueQueryHandler : IRequestHandler<GetLiveKpiValueQuery, LiveKpiValueDto>
{
    private readonly IKpiDefinitionRepository _kpiDefinitionRepository;
    private readonly IEnumerable<IKpiDataSource> _dataSources;

    public GetLiveKpiValueQueryHandler(IKpiDefinitionRepository kpiDefinitionRepository, IEnumerable<IKpiDataSource> dataSources)
    {
        _kpiDefinitionRepository = kpiDefinitionRepository;
        _dataSources = dataSources;
    }

    public async Task<LiveKpiValueDto> Handle(GetLiveKpiValueQuery request, CancellationToken cancellationToken)
    {
        var kpi = await _kpiDefinitionRepository.GetByIdAsync(request.CompanyId, request.KpiDefinitionId, cancellationToken)
            ?? throw new KeyNotFoundException($"KPI definition '{request.KpiDefinitionId}' was not found.");

        var dataSource = _dataSources.FirstOrDefault(ds => string.Equals(ds.KpiCode, kpi.Code, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException(
                $"No live data source is registered for KPI code '{kpi.Code}'. Supported codes: {string.Join(", ", _dataSources.Select(ds => ds.KpiCode))}.");

        var value = await dataSource.GetLiveValueAsync(request.CompanyId, cancellationToken);
        return new LiveKpiValueDto(kpi.Id, kpi.Code, kpi.Name, value, DateTimeOffset.UtcNow);
    }
}
