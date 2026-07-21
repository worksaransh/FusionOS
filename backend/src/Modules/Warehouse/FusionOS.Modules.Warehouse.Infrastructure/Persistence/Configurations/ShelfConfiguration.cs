using FusionOS.Modules.Warehouse.Domain.Shelves;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Warehouse.Infrastructure.Persistence.Configurations;

public sealed class ShelfConfiguration : IEntityTypeConfiguration<Shelf>
{
    public void Configure(EntityTypeBuilder<Shelf> builder)
    {
        builder.ToTable("shelves");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken();
        builder.Ignore(x => x.RowVersion);
        builder.HasIndex(x => new { x.CompanyId, x.RackId, x.Code }).IsUnique();
        builder.Ignore(x => x.DomainEvents);
    }
}
