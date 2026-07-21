using FusionOS.Modules.Manufacturing.Domain.WorkOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Manufacturing.Infrastructure.Persistence.Configurations;

public sealed class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
{
    public void Configure(EntityTypeBuilder<WorkOrder> builder)
    {
        builder.ToTable("work_orders");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.QuantityToProduce).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.QuantityGoodProduced).HasColumnType("numeric(19,4)");
        builder.Property(x => x.QuantityScrapped).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.YieldPercentage).HasColumnType("numeric(5,2)");
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.Status });
        builder.HasIndex(x => x.BillOfMaterialsId);

        builder.HasMany(x => x.Components)
            .WithOne()
            .HasForeignKey("WorkOrderId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Components).HasField("_components").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class WorkOrderComponentConfiguration : IEntityTypeConfiguration<WorkOrderComponent>
{
    public void Configure(EntityTypeBuilder<WorkOrderComponent> builder)
    {
        builder.ToTable("work_order_components");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.QuantityRequired).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.QuantityIssued).HasColumnType("numeric(19,4)").IsRequired();
    }
}
