using FusionOS.Modules.Sales.Domain.CreditNotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Sales.Infrastructure.Persistence.Configurations;

/// <summary>numeric(19,4) convention — same as InvoiceLine/JournalEntryLine/SalesOrderLine/PurchaseOrderLine (04_DATABASE_GUIDELINES.md).</summary>
public sealed class CreditNoteLineConfiguration : IEntityTypeConfiguration<CreditNoteLine>
{
    public void Configure(EntityTypeBuilder<CreditNoteLine> builder)
    {
        builder.ToTable("credit_note_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.UnitPrice).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.LineTotal).HasColumnType("numeric(19,4)").IsRequired();
    }
}
