using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Finance.Domain.Accounts;
using FusionOS.Modules.Finance.Domain.BankAccounts;
using FusionOS.Modules.Finance.Domain.BankStatementLines;
using FusionOS.Modules.Finance.Domain.BudgetLines;
using FusionOS.Modules.Finance.Domain.Budgets;
using FusionOS.Modules.Finance.Domain.CostCenters;
using FusionOS.Modules.Finance.Domain.ExchangeRates;
using FusionOS.Modules.Finance.Domain.FixedAssets;
using FusionOS.Modules.Finance.Domain.JournalEntries;
using FusionOS.Modules.Finance.Domain.Payables;
using FusionOS.Modules.Finance.Domain.Receivables;
using FusionOS.Modules.Finance.Domain.Settings;
using FusionOS.Modules.Finance.Domain.TaxJurisdictions;
using FusionOS.Modules.Finance.Domain.TaxRates;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Persistence;

/// <summary>
/// Owns the "finance" schema (05_MODULE_ROADMAP.md Phase 2 — Financial Backbone).
/// Account (Chart of Accounts), JournalEntry (General Ledger), ArLedgerEntry
/// (minimal Accounts Receivable), CostCenter (M8a — Finance depth, master
/// data only, not yet attached to JournalEntryLine), TaxJurisdiction/
/// TaxRate (M8b — Finance depth, multi-jurisdiction tax master data, also
/// not yet attached to any transactional line), ApLedgerEntry (M8c —
/// minimal Accounts Payable, manual-entry only — see its own class doc
/// comment for why there's no consumer wiring it yet), and BankAccount/
/// BankStatementLine (M8d — Finance depth: bank reconciliation, master data
/// plus manually-entered statement lines with a manual reconcile/unreconcile
/// toggle — see BankStatementLine's own class doc comment for the two
/// deliberate scope-outs: no bank-feed/file-import integration, no
/// auto-matching algorithm), ExchangeRate (M8e — Finance depth:
/// multi-currency support, dated FX rate master data plus a pure conversion
/// query — see ExchangeRate's own class doc comment for why no existing
/// aggregate carries a CurrencyCode field yet), and Budget/BudgetLine (M8f —
/// Finance depth: budgeting, master data plus a read-only actual-vs-budget
/// query over posted JournalEntry data — see Budget's and
/// GetBudgetVsActualQueryHandler's own doc comments for the scope line and
/// the cost-center-level-actuals limitation), and FixedAsset (M8g — Finance
/// depth: fixed assets, master data plus a pure on-demand straight-line
/// depreciation calculation — see FixedAsset's and
/// GetDepreciationScheduleQueryHandler's own doc comments for the scope line:
/// no automated depreciation-posting run, no disposal gain/loss GL posting,
/// nothing about depreciation history is persisted anywhere).
/// </summary>
public sealed class FinanceDbContext : BaseDbContext
{
    public FinanceDbContext(DbContextOptions<FinanceDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<ArLedgerEntry> ArLedgerEntries => Set<ArLedgerEntry>();
    public DbSet<CostCenter> CostCenters => Set<CostCenter>();
    public DbSet<TaxJurisdiction> TaxJurisdictions => Set<TaxJurisdiction>();
    public DbSet<TaxRate> TaxRates => Set<TaxRate>();
    public DbSet<ApLedgerEntry> ApLedgerEntries => Set<ApLedgerEntry>();
    public DbSet<BankAccount> BankAccounts => Set<BankAccount>();
    public DbSet<BankStatementLine> BankStatementLines => Set<BankStatementLine>();
    public DbSet<ExchangeRate> ExchangeRates => Set<ExchangeRate>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<BudgetLine> BudgetLines => Set<BudgetLine>();
    public DbSet<FixedAsset> FixedAssets => Set<FixedAsset>();
    public DbSet<FinanceSettings> FinanceSettings => Set<FinanceSettings>();
    public DbSet<PurchaseOrderFact> PurchaseOrderFacts => Set<PurchaseOrderFact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("finance");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
