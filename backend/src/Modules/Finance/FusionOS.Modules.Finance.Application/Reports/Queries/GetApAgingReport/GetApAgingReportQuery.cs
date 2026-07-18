using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Reports.Contracts;

namespace FusionOS.Modules.Finance.Application.Reports.Queries.GetApAgingReport;

/// <summary>Read-gated on the same permission as every other Payables read — a canned report is still just a read, mirrors GetArAgingReportQuery.</summary>
public sealed record GetApAgingReportQuery(Guid CompanyId) : IQuery<ApAgingReportDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.payable.read" };
}
