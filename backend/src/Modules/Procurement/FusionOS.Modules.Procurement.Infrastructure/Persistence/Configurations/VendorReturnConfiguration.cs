using FusionOS.Modules.Procurement.Domain.VendorReturns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Procurement.Infrastructure.Persistence.Configurations;

public sealed class VendorReturnConfiguration : IEntityTypeConfiguration<VendorReturn>
{
    public void Configure(EntityTypeBuilder<VendorReturn> builder)
    {
        builder.ToTable("vendor_returns");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Quantity).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.PurchaseOrderId, x.ProductId }); // CreateVendorReturnCommandHandler's own lookup shape
    }
}
