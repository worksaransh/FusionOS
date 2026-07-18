using FusionOS.Modules.Warehouse.Domain.CycleCounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Warehouse.Infrastructure.Persistence.Configurations;

public sealed class CycleCountConfiguration : IEntityTypeConfiguration<CycleCount>
{
    public void Configure(EntityTypeBuilder<CycleCount> builder)
    {
        builder.ToTable("cycle_counts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.SystemQuantitySnapshot).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.CountedQuantity).HasColumnType("numeric(19,4)");
        builder.Property(x => x.VarianceQuantity).HasColumnType("numeric(19,4)");
        builder.UseXminAsConcurrencyToken();
        builder.Ignore(x => x.RowVersion);
        builder.HasIndex(x => new { x.CompanyId, x.WarehouseId });
        builder.HasIndex(x => new { x.CompanyId, x.BinId });
        builder.Ignore(x => x.DomainEvents);
    }
}
