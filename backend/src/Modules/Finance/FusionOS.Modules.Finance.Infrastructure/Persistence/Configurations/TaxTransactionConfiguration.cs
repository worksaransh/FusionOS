using FusionOS.Modules.Finance.Domain.TaxTransactions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Finance.Infrastructure.Persistence.Configurations;

public sealed class TaxTransactionConfiguration : IEntityTypeConfiguration<TaxTransaction>
{
    public void Configure(EntityTypeBuilder<TaxTransaction> builder)
    {
        builder.ToTable("tax_transactions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Direction).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.TaxAmount).HasColumnType("numeric(18,2)").IsRequired();
        builder.Property(x => x.SourceEntityType).HasMaxLength(100).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(x => x.RowVersion);
        builder.HasIndex(x => new { x.CompanyId, x.TransactionDate });
        builder.HasIndex(x => new { x.CompanyId, x.TaxRateId });
        builder.HasIndex(x => new { x.CompanyId, x.SourceEntityType, x.SourceEntityId, x.TaxRateId }).IsUnique();
        builder.Ignore(x => x.DomainEvents);
    }
}
