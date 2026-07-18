using FusionOS.Modules.Inventory.Domain.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Inventory.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Sku).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.UnitOfMeasure).HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(p => p.RowVersion);
        builder.HasIndex(p => new { p.CompanyId, p.Sku }).IsUnique();
        builder.Ignore(p => p.DomainEvents);

        // M9-remaining e: Multi-UOM (2026-07-16) — entity-with-own-Id child collection,
        // mirroring GoodsReceiptLine's HasMany/WithOne shape (no OwnsMany precedent in this codebase).
        builder.HasMany(p => p.UnitOfMeasureConversions)
            .WithOne()
            .HasForeignKey("ProductId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.UnitOfMeasureConversions).HasField("_unitOfMeasureConversions").UsePropertyAccessMode(PropertyAccessMode.Field);

        // Phase 1 closeout (2026-07-18): Variants — same entity-with-own-Id child collection shape as UnitOfMeasureConversions above.
        builder.HasMany(p => p.Variants)
            .WithOne()
            .HasForeignKey("ProductId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Variants).HasField("_variants").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
