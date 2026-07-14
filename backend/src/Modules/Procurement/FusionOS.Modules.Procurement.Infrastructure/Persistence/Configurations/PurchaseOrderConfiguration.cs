using FusionOS.Modules.Procurement.Domain.PurchaseOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Procurement.Infrastructure.Persistence.Configurations;

public sealed class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("purchase_orders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.TotalAmount);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.SupplierId });

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey("PurchaseOrderId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Lines).HasField("_lines").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
