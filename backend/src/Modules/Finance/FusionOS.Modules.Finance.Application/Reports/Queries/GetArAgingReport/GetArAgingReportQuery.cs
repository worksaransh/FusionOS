using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Reports.Contracts;

namespace FusionOS.Modules.Finance.Application.Reports.Queries.GetArAgingReport;

/// <summary>Read-gated on the same permission as every other Receivables read (Phase M6, 2026-07-15) — a canned report is still just a read.</summary>
public sealed record GetArAgingReportQuery(Guid CompanyId) : IQuery<ArAgingReportDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.receivable.read" };
}
