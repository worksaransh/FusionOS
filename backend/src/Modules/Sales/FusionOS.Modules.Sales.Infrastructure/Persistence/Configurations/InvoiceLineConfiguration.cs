using FusionOS.Modules.Sales.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Sales.Infrastructure.Persistence.Configurations;

/// <summary>
/// Fixes a confirmed audit gap (2026-07 sprint): InvoiceLine's decimal columns had
/// no explicit precision/scale configured, so SQL Server/Postgres would otherwise
/// fall back to a default that silently truncates fractional values. Matches the
/// numeric(19,4) convention established by JournalEntryLine/SalesOrderLine/
/// PurchaseOrderLine (04_DATABASE_GUIDELINES.md).
/// </summary>
public sealed class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("invoice_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.UnitPrice).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.LineTotal).HasColumnType("numeric(19,4)").IsRequired();
        // Optional cross-module Finance TaxRate reference + the stored tax amount for
        // this line (numeric(19,4), same scale as the money columns above). No FK
        // constraint: TaxRateId is an opaque cross-module reference, same as ProductId.
        builder.Property(x => x.TaxRateId);
        builder.Property(x => x.TaxAmount).HasColumnType("numeric(19,4)").IsRequired();
        builder.HasIndex(x => x.TaxRateId);
    }
}
