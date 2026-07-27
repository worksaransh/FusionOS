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
///
/// <b>DefaultDisposalProceedsAccountId/DefaultGainLossOnDisposalAccountId
/// (2026-07-22)</b> — added additively, same nullable/get-or-create-safe
/// shape as the original four, for DisposeFixedAssetCommandHandler's own
/// disposal gain/loss GL posting: DefaultDisposalProceedsAccountId is the
/// cash/receivable account debited for any disposal proceeds received (only
/// needed when a disposal actually has nonzero proceeds), and
/// DefaultGainLossOnDisposalAccountId is the account debited (loss) or
/// credited (gain) for the computed gain/loss (only needed when that
/// disposal's proceeds differ from the asset's net book value). Existing
/// rows/callers are unaffected — both default to null until configured, same
/// "silently skip that part of the GL post" convention as the original four.
///
/// <b>DefaultSalaryExpenseAccountId/DefaultSalaryPayableAccountId
/// (2026-07-23)</b> — added additively, same nullable/get-or-create-safe
/// shape, for HRMS's PayrollRecordPaid event: PayrollRecordPaidConsumer posts
/// a balanced Debit Salary Expense / Credit Salary Payable JournalEntry for
/// GrossPay when a payroll record is marked paid. Configured via a separate
/// <see cref="ConfigurePayrollAccounts"/> method rather than adding more
/// parameters to <see cref="ConfigureAccounts"/> — that method's shape is the
/// existing settings-update command's shape, and salary accounts are a
/// distinct concern (HRMS, not Sales/Procurement/FixedAssets) with no reason
/// to be coupled to that call's parameter list. Same "silently skip the GL
/// post while either account is unconfigured" convention as the other pairs.
/// </summary>
public sealed class FinanceSettings : TenantAggregateRoot
{
    public Guid? DefaultArAccountId { get; private set; }
    public Guid? DefaultSalesRevenueAccountId { get; private set; }
    public Guid? DefaultApAccountId { get; private set; }
    public Guid? DefaultPurchaseExpenseAccountId { get; private set; }
    public Guid? DefaultDisposalProceedsAccountId { get; private set; }
    public Guid? DefaultGainLossOnDisposalAccountId { get; private set; }
    public Guid? DefaultSalaryExpenseAccountId { get; private set; }
    public Guid? DefaultSalaryPayableAccountId { get; private set; }

    private FinanceSettings() { }

    public static FinanceSettings CreateDefault(Guid companyId)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("Company id is required.", nameof(companyId));

        return new FinanceSettings { CompanyId = companyId };
    }

    public void ConfigureAccounts(
        Guid? defaultArAccountId,
        Guid? defaultSalesRevenueAccountId,
        Guid? defaultApAccountId,
        Guid? defaultPurchaseExpenseAccountId,
        Guid? defaultDisposalProceedsAccountId = null,
        Guid? defaultGainLossOnDisposalAccountId = null)
    {
        DefaultArAccountId = defaultArAccountId;
        DefaultSalesRevenueAccountId = defaultSalesRevenueAccountId;
        DefaultApAccountId = defaultApAccountId;
        DefaultPurchaseExpenseAccountId = defaultPurchaseExpenseAccountId;
        DefaultDisposalProceedsAccountId = defaultDisposalProceedsAccountId;
        DefaultGainLossOnDisposalAccountId = defaultGainLossOnDisposalAccountId;
    }

    /// <summary>
    /// Configures the salary GL accounts PayrollRecordPaidConsumer needs to
    /// post its Debit Salary Expense / Credit Salary Payable JournalEntry. A
    /// separate method (not folded into <see cref="ConfigureAccounts"/>) — see
    /// this class's own doc comment for why. Both null (the default) keeps
    /// today's "no GL post" behavior for payroll payments.
    /// </summary>
    public void ConfigurePayrollAccounts(Guid? salaryExpenseAccountId, Guid? salaryPayableAccountId)
    {
        DefaultSalaryExpenseAccountId = salaryExpenseAccountId;
        DefaultSalaryPayableAccountId = salaryPayableAccountId;
    }
}
