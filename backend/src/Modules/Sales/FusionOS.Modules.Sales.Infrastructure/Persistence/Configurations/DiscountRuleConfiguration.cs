using FusionOS.Modules.Sales.Domain.Discounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class DiscountRuleConfiguration : IEntityTypeConfiguration<DiscountRule>
{
    public void Configure(EntityTypeBuilder<DiscountRule> builder)
    {
        builder.ToTable("discount_rules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MinQuantity).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.DiscountPercentage).HasColumnType("numeric(5,2)").IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.ProductId, x.IsActive }); // GetApplicableDiscountQuery's own lookup shape
    }
}
