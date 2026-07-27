using FusionOS.Modules.Procurement.Domain.SupplierRatings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Procurement.Infrastructure.Persistence.Configurations;

public sealed class SupplierRatingConfiguration : IEntityTypeConfiguration<SupplierRating>
{
    public void Configure(EntityTypeBuilder<SupplierRating> builder)
    {
        builder.ToTable("supplier_ratings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Comments).HasMaxLength(2000);
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.SupplierId });
        builder.HasIndex(x => new { x.CompanyId, x.PurchaseOrderId });
    }
}
