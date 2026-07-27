using FusionOS.Modules.Crm.Application.Dashboard.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Application.Opportunities.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Dashboard.Queries.GetCrmDashboard;

/// <summary>
/// Read-only aggregation query — all three metrics (pipeline by stage, win rate, lead
/// conversion rate) come from repository methods that GroupBy/Count server-side
/// (IOpportunityRepository.GetOpenPipelineByStageAsync/GetClosedCountsAsync,
/// ILeadRepository.GetConversionCountsAsync), the same "aggregate in the database, shape
/// the small result in the handler" convention as Finance's GetApAgingReportQueryHandler —
/// no full opportunity/lead table scan into memory.
/// </summary>
public sealed class GetCrmDashboardQueryHandler : IRequestHandler<GetCrmDashboardQuery, CrmDashboardDto>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly ILeadRepository _leadRepository;

    public GetCrmDashboardQueryHandler(IOpportunityRepository opportunityRepository, ILeadRepository leadRepository)
    {
        _opportunityRepository = opportunityRepository;
        _leadRepository = leadRepository;
    }

    public async Task<CrmDashboardDto> Handle(GetCrmDashboardQuery request, CancellationToken cancellationToken)
    {
        var pipeline = await _opportunityRepository.GetOpenPipelineByStageAsync(request.CompanyId, cancellationToken);
        var (wonCount, lostCount) = await _opportunityRepository.GetClosedCountsAsync(request.CompanyId, cancellationToken);
        var (totalLeadCount, convertedLeadCount) = await _leadRepository.GetConversionCountsAsync(request.CompanyId, cancellationToken);

        var pipelineByStage = pipeline
            .Select(p => new PipelineStageDto(p.Stage.ToString(), p.EstimatedValueTotal, p.WeightedValueTotal))
            .OrderBy(p => p.Stage)
            .ToList();

        var closedCount = wonCount + lostCount;
        var winRatePercent = closedCount == 0 ? 0m : Math.Round(wonCount * 100m / closedCount, 2);
        var leadConversionRatePercent = totalLeadCount == 0 ? 0m : Math.Round(convertedLeadCount * 100m / totalLeadCount, 2);

        return new CrmDashboardDto(
            pipelineByStage,
            pipelineByStage.Sum(p => p.EstimatedValueTotal),
            pipelineByStage.Sum(p => p.WeightedValueTotal),
            wonCount,
            lostCount,
            winRatePercent,
            totalLeadCount,
            convertedLeadCount,
            leadConversionRatePercent);
    }
}
