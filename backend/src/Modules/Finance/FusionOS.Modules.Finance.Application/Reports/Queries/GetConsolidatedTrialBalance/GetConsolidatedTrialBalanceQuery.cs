using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Reports.Contracts;

namespace FusionOS.Modules.Finance.Application.Reports.Queries.GetConsolidatedTrialBalance;

/// <summary>
/// Multi-entity/subsidiary consolidation (Phase 2.4): the group-level trial
/// balance for ParentCompanyId and every Company whose ParentCompanyId points
/// back to it. Only matters for customers with subsidiaries — a company with
/// no ParentCompanyId links pointing at it simply consolidates to itself, so
/// this is purely additive over the existing single-company GetTrialBalanceQuery,
/// which is untouched. Read-gated on its own permission (not
/// finance.journal-entry.read) because it reaches across every subsidiary's
/// ledger data, not just the caller's own company's.
/// </summary>
public sealed record GetConsolidatedTrialBalanceQuery(Guid ParentCompanyId, DateTimeOffset AsOf)
    : IQuery<ConsolidatedTrialBalanceReportDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.consolidated-trial-balance.read" };
}
