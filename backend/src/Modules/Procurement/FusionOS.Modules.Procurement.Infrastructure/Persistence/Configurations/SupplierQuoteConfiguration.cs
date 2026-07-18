using FusionOS.Modules.Procurement.Domain.Rfqs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Procurement.Infrastructure.Persistence.Configurations;

/// <summary>See SupplierQuote's doc comment for why this table has no audit/tenant columns of its own — owned entirely by the parent Rfq. SupplierId is stored as a plain column (same-module FK, validated in the handler, not via a DB-level FK — same convention as every other same-module reference in this codebase).</summary>
public sealed class SupplierQuoteConfiguration : IEntityTypeConfiguration<SupplierQuote>
{
    public void Configure(EntityTypeBuilder<SupplierQuote> builder)
    {
        builder.ToTable("supplier_quotes");
        builder.HasKey(x => x.Id);
        builder.Ignore(x => x.TotalAmount);
        builder.HasIndex(x => x.SupplierId);

        builder.HasMany(x => x.Lines)
            .WithOne()
            .HasForeignKey("SupplierQuoteId")
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(x => x.Lines).HasField("_lines").UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
