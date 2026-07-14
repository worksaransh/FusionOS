using FusionOS.Modules.Procurement.Domain.PurchaseOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Procurement.Infrastructure.Persistence.Configurations;

/// <summary>See PurchaseOrderLine's doc comment for why this table has no audit/tenant columns of its own.</summary>
public sealed class PurchaseOrderLineConfiguration : IEntityTypeConfiguration<PurchaseOrderLine>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
    {
        builder.ToTable("purchase_order_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.UnitPrice).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.LineTotal).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.ReceivedQuantity).HasColumnType("numeric(19,4)").IsRequired();
        builder.Ignore(x => x.IsFullyReceived);
    }
}
