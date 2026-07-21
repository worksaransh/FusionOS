using FusionOS.Modules.Finance.Domain.Payables;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Finance.Infrastructure.Persistence.Configurations;

/// <summary>Mutable projection table (see PurchaseOrderFact's class doc comment) - not append-only, unlike ApLedgerEntryConfiguration.</summary>
public sealed class PurchaseOrderFactConfiguration : IEntityTypeConfiguration<PurchaseOrderFact>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderFact> builder)
    {
        builder.ToTable("purchase_order_facts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.OrderedAmount).HasColumnType("numeric(19,4)");
        builder.Property(x => x.ReceivedAmount).HasColumnType("numeric(19,4)").IsRequired();
        builder.UseXminAsConcurrencyToken();
        builder.Ignore(x => x.RowVersion);
        builder.HasIndex(x => new { x.CompanyId, x.PurchaseOrderId }).IsUnique();
        builder.Ignore(x => x.DomainEvents);
    }
}
