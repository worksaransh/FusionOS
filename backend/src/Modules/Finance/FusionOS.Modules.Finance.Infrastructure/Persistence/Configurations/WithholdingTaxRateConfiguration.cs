using FusionOS.Modules.Finance.Domain.WithholdingTaxRates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Finance.Infrastructure.Persistence.Configurations;

public sealed class WithholdingTaxRateConfiguration : IEntityTypeConfiguration<WithholdingTaxRate>
{
    public void Configure(EntityTypeBuilder<WithholdingTaxRate> builder)
    {
        builder.ToTable("withholding_tax_rates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SectionCode).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Percentage).HasColumnType("numeric(5,2)").IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.SectionCode }).IsUnique();
    }
}
