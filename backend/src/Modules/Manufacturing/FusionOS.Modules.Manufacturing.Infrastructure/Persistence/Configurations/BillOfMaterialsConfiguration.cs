using FusionOS.Modules.Manufacturing.Domain.BillOfMaterials;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Manufacturing.Infrastructure.Persistence.Configurations;

public sealed class BillOfMaterialsConfiguration : IEntityTypeConfiguration<BillOfMaterials>
{
    public void Configure(EntityTypeBuilder<BillOfMaterials> builder)
    {
        builder.ToTable("bills_of_materials");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
        builder.HasIndex(x => x.ProductId);

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey("BillOfMaterialsId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Lines).HasField("_lines").UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.Operations)
            .WithOne()
            .HasForeignKey("BillOfMaterialsId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Operations).HasField("_operations").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

public sealed class BomLineConfiguration : IEntityTypeConfiguration<BomLine>
{
    public void Configure(EntityTypeBuilder<BomLine> builder)
    {
        builder.ToTable("bom_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasColumnType("numeric(19,4)").IsRequired();
    }
}

public sealed class RoutingOperationConfiguration : IEntityTypeConfiguration<RoutingOperation>
{
    public void Configure(EntityTypeBuilder<RoutingOperation> builder)
    {
        builder.ToTable("routing_operations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SequenceNumber).IsRequired();
        builder.Property(x => x.OperationName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.WorkCenter).HasMaxLength(100).IsRequired();
        builder.Property(x => x.StandardMinutes).HasColumnType("numeric(19,4)").IsRequired();
    }
}
