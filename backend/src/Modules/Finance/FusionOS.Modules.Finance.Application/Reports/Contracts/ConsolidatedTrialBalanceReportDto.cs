namespace FusionOS.Modules.Finance.Application.Reports.Contracts;

/// <summary>
/// One GL account CODE's consolidated position across every entity rolled into
/// the group (the parent company plus every subsidiary reachable via
/// Company.ParentCompanyId) — see GetConsolidatedTrialBalanceQueryHandler.
/// Summed by account CODE rather than AccountId because each entity keeps its
/// own chart-of-accounts Account rows (Account is TenantAggregateRoot-scoped);
/// the code is the only thing shared across every entity's chart.
/// IsEliminationAdjusted is true when an active IntercompanyEliminationRule
/// matched this code as one side of a receivable/payable pair and the totals
/// below already reflect that netting — false means these are the raw summed
/// totals with no elimination applied.
/// </summary>
public sealed record ConsolidatedTrialBalanceLineDto(
    string AccountCode,
    decimal TotalDebit,
    decimal TotalCredit,
    decimal NetBalance,
    bool IsEliminationAdjusted);

/// <summary>
/// The consolidated trial balance as of a date: every GL account code across
/// the parent company and its subsidiaries, netted against active
/// IntercompanyEliminationRule pairs, plus the group-level grand totals.
/// CompanyIdsIncluded is the parent company's Id followed by every subsidiary
/// Id actually rolled up — lets a caller confirm which entities contributed
/// before trusting the totals. IsBalanced restates the double-entry invariant
/// at the consolidated level, same rationale as TrialBalanceReportDto.IsBalanced.
/// </summary>
public sealed record ConsolidatedTrialBalanceReportDto(
    DateTimeOffset AsOf,
    Guid ParentCompanyId,
    IReadOnlyList<Guid> CompanyIdsIncluded,
    IReadOnlyList<ConsolidatedTrialBalanceLineDto> Lines,
    decimal TotalDebit,
    decimal TotalCredit,
    bool IsBalanced);
