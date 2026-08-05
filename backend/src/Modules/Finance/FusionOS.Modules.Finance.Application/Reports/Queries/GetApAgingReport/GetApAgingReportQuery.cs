using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Reports.Contracts;

namespace FusionOS.Modules.Finance.Application.Reports.Queries.GetApAgingReport;

/// <summary>
/// Read-gated on the same permission as every other Payables read — a canned report is still just a read, mirrors GetArAgingReportQuery.
/// FromDate/ToDate (Phase 2.3) are optional and non-breaking: when omitted, behavior is unchanged (all-time); when supplied they filter
/// lines to supplier balances whose OldestChargeDate falls within the range, for the Executive Dashboard's date-range picker.
/// </summary>
public sealed record GetApAgingReportQuery(Guid CompanyId, DateTimeOffset? FromDate = null, DateTimeOffset? ToDate = null) : IQuery<ApAgingReportDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.payable.read" };
}
