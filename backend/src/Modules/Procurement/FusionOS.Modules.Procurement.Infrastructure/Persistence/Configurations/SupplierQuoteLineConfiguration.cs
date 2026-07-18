using FusionOS.Modules.Procurement.Domain.Rfqs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Procurement.Infrastructure.Persistence.Configurations;

/// <summary>numeric(19,4) convention — same as PurchaseOrderLine/RfqLine (04_DATABASE_GUIDELINES.md).</summary>
public sealed class SupplierQuoteLineConfiguration : IEntityTypeConfiguration<SupplierQuoteLine>
{
    public void Configure(EntityTypeBuilder<SupplierQuoteLine> builder)
    {
        builder.ToTable("supplier_quote_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.UnitPrice).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.LineTotal).HasColumnType("numeric(19,4)").IsRequired();
    }
}
