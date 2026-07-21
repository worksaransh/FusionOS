using FusionOS.Modules.Warehouse.Domain.Packages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Warehouse.Infrastructure.Persistence.Configurations;

/// <summary>
/// Same "line entity gets its own precision-configured EntityTypeConfiguration" shape as
/// DispatchLineConfiguration (Sales) — numeric(19,4), the codebase-wide quantity/amount
/// convention (04_DATABASE_GUIDELINES.md), applied here from the start rather than left as a gap.
/// </summary>
public sealed class PackageLineConfiguration : IEntityTypeConfiguration<PackageLine>
{
    public void Configure(EntityTypeBuilder<PackageLine> builder)
    {
        builder.ToTable("package_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasColumnType("numeric(19,4)").IsRequired();
    }
}
