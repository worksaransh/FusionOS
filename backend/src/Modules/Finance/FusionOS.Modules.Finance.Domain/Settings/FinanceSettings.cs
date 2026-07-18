using FusionOS.SharedKernel;

namespace FusionOS.Modules.Finance.Domain.Settings;

/// <summary>
/// One row per company (Phase 2 closeout, 2026-07-18) — get-or-create
/// singleton, same pattern as Core's `CompanySettings`. Holds the default
/// Chart-of-Accounts mapping Finance's own integration-event consumers
/// (InvoiceIssuedConsumer, CreditNoteIssuedConsumer,
/// PurchaseOrderGoodsReceiptCostedConsumer) need to auto-post a balanced
/// JournalEntry alongside the AR/AP subledger entry they already write —
/// closing the gap where invoices/bills updated the subledgers but never
/// touched the GL, so Trial Balance/P&amp;L/Balance Sheet never reflected them.
///
/// All four fields are nullable and start unset: a company that hasn't
/// configured its default accounts yet keeps today's subledger-only
/// behavior (the consumers check for null and skip the GL post) rather than
/// failing or guessing at an account. GL posting turns on automatically the
/// moment a Finance admin configures these, with no code change needed.
///
/// AccountIds are in-module references, validated for existence by
/// UpdateFinanceSettingsCommandHandler at configuration time — the consumers
/// themselves trust an already-configured id without re-validating it, same
/// restraint as PostMonthlyDepreciationCommandHandler validating once at
/// command time rather than the domain re-checking on every post.
/// </summary>
public sealed class FinanceSettings : TenantAggregateRoot
{
    public Guid? DefaultArAccountId { get; private set; }
    public Guid? DefaultSalesRevenueAccountId { get; private set; }
    public Guid? DefaultApAccountId { get; private set; }
    public Guid? DefaultPurchaseExpenseAccountId { get; private set; }

    private FinanceSettings() { }

    public static FinanceSettings CreateDefault(Guid companyId)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("Company id is required.", nameof(companyId));

        return new FinanceSettings { CompanyId = companyId };
    }

    public void ConfigureAccounts(Guid? defaultArAccountId, Guid? defaultSalesRevenueAccountId, Guid? defaultApAccountId, Guid? defaultPurchaseExpenseAccountId)
    {
        DefaultArAccountId = defaultArAccountId;
        DefaultSalesRevenueAccountId = defaultSalesRevenueAccountId;
        DefaultApAccountId = defaultApAccountId;
        DefaultPurchaseExpenseAccountId = defaultPurchaseExpenseAccountId;
    }
}
