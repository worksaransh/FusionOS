using FusionOS.Modules.Warehouse.Domain.Bins;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Warehouse.Infrastructure.Persistence.Configurations;

public sealed class BinConfiguration : IEntityTypeConfiguration<Bin>
{
    public void Configure(EntityTypeBuilder<Bin> builder)
    {
        builder.ToTable("bins");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken();
        builder.Ignore(x => x.RowVersion);
        builder.HasIndex(x => new { x.CompanyId, x.ZoneId, x.Code }).IsUnique();
        // Additive — Bin's optional Shelf refinement (Bin.ShelfId), non-unique since not every bin has one.
        builder.HasIndex(x => x.ShelfId);
        builder.Ignore(x => x.DomainEvents);
    }
}
