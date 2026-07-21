using FusionOS.Modules.Inventory.Domain.SerialUnits;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class SerialUnitConfiguration : IEntityTypeConfiguration<SerialUnit>
{
    public void Configure(EntityTypeBuilder<SerialUnit> builder)
    {
        builder.ToTable("serial_units");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SerialNumber).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.ProductId, x.SerialNumber }).IsUnique();
        builder.HasIndex(x => new { x.CompanyId, x.SerialNumber }); // GetSerialUnitBySerialNumberQuery's own lookup shape — the "scan a serial" use case
    }
}
