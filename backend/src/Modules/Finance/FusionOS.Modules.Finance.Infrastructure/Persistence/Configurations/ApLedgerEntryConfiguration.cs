using FusionOS.Modules.Finance.Domain.Payables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Finance.Infrastructure.Persistence.Configurations;

/// <summary>Append-only per 04_DATABASE_GUIDELINES.md §12 — no application code path ever issues an UPDATE or DELETE against this table. Mirrors ArLedgerEntryConfiguration exactly, except PurchaseOrderId is nullable (see ApLedgerEntry's class doc comment).</summary>
public sealed class ApLedgerEntryConfiguration : IEntityTypeConfiguration<ApLedgerEntry>
{
    public void Configure(EntityTypeBuilder<ApLedgerEntry> builder)
    {
        builder.ToTable("ap_ledger_entries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Amount).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(x => x.RowVersion);
        builder.HasIndex(x => new { x.CompanyId, x.SupplierId });
        builder.HasIndex(x => x.PurchaseOrderId);
        builder.Ignore(x => x.DomainEvents);
    }
}
