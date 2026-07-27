using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Ai.Application.Reports.Contracts;

namespace FusionOS.Modules.Ai.Application.Reports.Queries.GenerateKpiNarrativeReport;

/// <summary>
/// Generates a narrative report from an existing KPI's own snapshot history
/// (12_AI_PLATFORM.md §4 "Automated report generation"). Reads Business
/// Intelligence's KpiDefinition/KpiSnapshot data exclusively through BI's own
/// MediatR queries (<c>GetKpiDefinitionByIdQuery</c>/<c>ListKpiSnapshotsQuery</c>)
/// via <c>ISender</c> — never a direct reference to BI's Domain types or a
/// direct DbContext query — so BI's own <c>IRequirePermission</c> gates still
/// apply on top of this query's own "ai.report.generate" gate.
/// </summary>
public sealed record GenerateKpiNarrativeReportQuery(Guid CompanyId, Guid KpiDefinitionId, int LookbackDays = 90)
    : IQuery<KpiNarrativeReportDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "ai.report.generate" };
}
