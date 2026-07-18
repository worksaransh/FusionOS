using FusionOS.Modules.Finance.Domain.BankStatementLines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Finance.Infrastructure.Persistence.Configurations;

/// <summary>Unlike ApLedgerEntryConfiguration, this table is not append-only — Reconcile/Unreconcile issue real UPDATEs against IsReconciled/ReconciledAt/MatchedJournalEntryId (04_DATABASE_GUIDELINES.md §12 only restricts genuinely append-only ledgers).</summary>
public sealed class BankStatementLineConfiguration : IEntityTypeConfiguration<BankStatementLine>
{
    public void Configure(EntityTypeBuilder<BankStatementLine> builder)
    {
        builder.ToTable("bank_statement_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(x => x.RowVersion);
        builder.HasIndex(x => new { x.CompanyId, x.BankAccountId }); // no uniqueness — multiple lines can share a date/amount
        builder.Ignore(x => x.DomainEvents);
    }
}
