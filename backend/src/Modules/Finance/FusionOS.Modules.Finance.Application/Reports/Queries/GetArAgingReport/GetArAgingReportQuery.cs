using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Reports.Contracts;

namespace FusionOS.Modules.Finance.Application.Reports.Queries.GetArAgingReport;

/// <summary>
/// Read-gated on the same permission as every other Receivables read (Phase M6, 2026-07-15) — a canned report is still just a read.
/// FromDate/ToDate (Phase 2.3) are optional and non-breaking: when omitted, behavior is unchanged (all-time, matching every existing caller);
/// when supplied they filter lines to invoices whose ChargeDate falls within the range, for the Executive Dashboard's date-range picker.
/// </summary>
public sealed record GetArAgingReportQuery(Guid CompanyId, DateTimeOffset? FromDate = null, DateTimeOffset? ToDate = null) : IQuery<ArAgingReportDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.receivable.read" };
}
