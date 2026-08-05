using FusionOS.Modules.Finance.Domain.WithholdingTaxTransactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Finance.Infrastructure.Persistence.Configurations;

public sealed class WithholdingTaxTransactionConfiguration : IEntityTypeConfiguration<WithholdingTaxTransaction>
{
    public void Configure(EntityTypeBuilder<WithholdingTaxTransaction> builder)
    {
        builder.ToTable("withholding_tax_transactions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ReferenceType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.GrossAmount).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.DeductedAmount).HasColumnType("numeric(19,4)").IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => x.WithholdingTaxRateId);
        builder.HasIndex(x => new { x.ReferenceType, x.ReferenceId });
    }
}
