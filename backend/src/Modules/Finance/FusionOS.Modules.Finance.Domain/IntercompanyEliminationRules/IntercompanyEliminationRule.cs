using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.IntercompanyEliminationRules;

/// <summary>
/// A GL account CODE pair that must net to zero across a consolidated group
/// (Phase 2.4 — multi-entity/subsidiary consolidation). For example, the
/// "Intercompany Receivable" code booked on a parent against a subsidiary
/// should exactly offset the "Intercompany Payable" code booked on that
/// subsidiary against the parent — GetConsolidatedTrialBalanceQuery nets these
/// out of the group total rather than double-counting the same intercompany
/// transaction as both an asset and a liability at the consolidated level.
///
/// Modeled by account CODE (e.g. "1310", "2310"), not by AccountId, because
/// each subsidiary Company has its own Account rows in Finance's chart of
/// accounts (Account is TenantAggregateRoot-scoped) — the code is the only
/// thing shared across every entity's chart, same reasoning
/// GetConsolidatedTrialBalanceQuery itself uses to sum subsidiary balances by
/// code rather than by AccountId. Owned by (created under) the parent company
/// — CompanyId is the parent's, per TenantAggregateRoot's normal scoping.
/// </summary>
public sealed class IntercompanyEliminationRule : TenantAggregateRoot
{
    public string ReceivableAccountCode { get; private set; } = default!;
    public string PayableAccountCode { get; private set; } = default!;
    public string Description { get; private set; } = default!;
    public bool IsActive { get; private set; }

    private IntercompanyEliminationRule() { }

    public static IntercompanyEliminationRule Create(
        Guid companyId,
        string receivableAccountCode,
        string payableAccountCode,
        string description,
        Guid? branchId = null)
    {
        if (string.IsNullOrWhiteSpace(receivableAccountCode))
            throw new ArgumentException("Receivable account code is required.", nameof(receivableAccountCode));
        if (string.IsNullOrWhiteSpace(payableAccountCode))
            throw new ArgumentException("Payable account code is required.", nameof(payableAccountCode));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description is required.", nameof(description));
        if (string.Equals(receivableAccountCode.Trim(), payableAccountCode.Trim(), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Receivable and payable account codes must differ.", nameof(payableAccountCode));

        return new IntercompanyEliminationRule
        {
            CompanyId = companyId,
            BranchId = branchId,
            // Uppercased to match Account.Code's own normalization (Account.Create) — the
            // consolidated trial balance handler looks these codes up in a dictionary keyed
            // by the normalized Account.Code via case-sensitive Ordinal comparison, so any
            // case mismatch here silently drops the elimination (debits still equal credits
            // overall, so IsBalanced stays true — the failure is invisible unless you check
            // IsEliminationAdjusted specifically).
            ReceivableAccountCode = receivableAccountCode.Trim().ToUpperInvariant(),
            PayableAccountCode = payableAccountCode.Trim().ToUpperInvariant(),
            Description = description.Trim(),
            IsActive = true,
        };
    }

    public void Deactivate() => IsActive = false;
}
