using FusionOS.Modules.Inventory.Domain.Ledger;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Inventory.Infrastructure.Persistence.Configurations;

/// <summary>Append-only per 04_DATABASE_GUIDELINES.md §12 — no application code path ever issues an UPDATE or DELETE against this table.</summary>
public sealed class InventoryLedgerEntryConfiguration : IEntityTypeConfiguration<InventoryLedgerEntry>
{
    public void Configure(EntityTypeBuilder<InventoryLedgerEntry> builder)
    {
        builder.ToTable("inventory_ledger_entries");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.QuantityDelta).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.UnitCost).HasColumnType("numeric(19,4)");
        builder.Property(x => x.BatchNumber).HasMaxLength(100);
        builder.Property(x => x.SerialNumber).HasMaxLength(100);
        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(x => x.RowVersion);
        builder.HasIndex(x => new { x.CompanyId, x.ProductId, x.WarehouseId });
        builder.HasIndex(x => new { x.CompanyId, x.ProductId, x.TransactionDate });
        builder.HasIndex(x => new { x.CompanyId, x.SerialNumber }); // traceability lookup: "where is this serialized unit"
        builder.HasIndex(x => new { x.CompanyId, x.BatchNumber }); // traceability lookup: "which movements touched this batch/lot"
        builder.Ignore(x => x.DomainEvents);
    }
}
