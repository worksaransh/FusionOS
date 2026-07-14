using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Warehouse.Infrastructure.Persistence.Configurations;

public sealed class WarehouseConfiguration : IEntityTypeConfiguration<Domain.Warehouses.Warehouse>
{
    public void Configure(EntityTypeBuilder<Domain.Warehouses.Warehouse> builder)
    {
        builder.ToTable("warehouses");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Address).HasMaxLength(300);
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(x => x.RowVersion);
        builder.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
        builder.Ignore(x => x.DomainEvents);
    }
}
