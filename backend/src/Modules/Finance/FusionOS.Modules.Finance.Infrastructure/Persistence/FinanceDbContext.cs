using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Finance.Domain.Accounts;
using FusionOS.Modules.Finance.Domain.JournalEntries;
using FusionOS.Modules.Finance.Domain.Receivables;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Persistence;

/// <summary>
/// Owns the "finance" schema (05_MODULE_ROADMAP.md Phase 2 — Financial Backbone).
/// Account (Chart of Accounts), JournalEntry (General Ledger), and ArLedgerEntry
/// (minimal Accounts Receivable). AP, GST/Taxes, cost centers, and bank
/// reconciliation are still reserved for later slices.
/// </summary>
public sealed class FinanceDbContext : BaseDbContext
{
    public FinanceDbContext(DbContextOptions<FinanceDbContext> options, ICurrentUserContext currentUser)
        : base(options, currentUser) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalEntry> JournalEntries => Set<JournalEntry>();
    public DbSet<ArLedgerEntry> ArLedgerEntries => Set<ArLedgerEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("finance");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
