using FusionOS.Modules.Warehouse.Domain.Packages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Warehouse.Infrastructure.Persistence.Configurations;

/// <summary>
/// numeric(19,4) on every decimal column matches the reviewed convention established by
/// JournalEntryLine/SalesOrderLine/PurchaseOrderLine and fixed onto DispatchLine
/// (04_DATABASE_GUIDELINES.md) — deliberately not left unconfigured the way PickListLine's own
/// quantity columns still are (a known, separate pre-existing gap, out of scope here).
/// </summary>
public sealed class PackageConfiguration : IEntityTypeConfiguration<Package>
{
    public void Configure(EntityTypeBuilder<Package> builder)
    {
        builder.ToTable("packages");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.PackageNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.WeightKg).HasColumnType("numeric(19,4)");
        builder.Property(x => x.LengthCm).HasColumnType("numeric(19,4)");
        builder.Property(x => x.WidthCm).HasColumnType("numeric(19,4)");
        builder.Property(x => x.HeightCm).HasColumnType("numeric(19,4)");
        builder.UseXminAsConcurrencyToken();
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.PickListId });
        builder.HasIndex(x => new { x.CompanyId, x.PickListId, x.PackageNumber }).IsUnique();

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey("PackageId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Lines).HasField("_lines").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
