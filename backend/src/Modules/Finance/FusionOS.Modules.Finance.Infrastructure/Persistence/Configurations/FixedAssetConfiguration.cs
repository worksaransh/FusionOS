using FusionOS.Modules.Finance.Domain.FixedAssets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Finance.Infrastructure.Persistence.Configurations;

public sealed class FixedAssetConfiguration : IEntityTypeConfiguration<FixedAsset>
{
    public void Configure(EntityTypeBuilder<FixedAsset> builder)
    {
        builder.ToTable("fixed_assets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        // numeric(19,4) — a money amount, not an FX rate, same precision
        // BudgetLine.BudgetedAmount/BankStatementLine.Amount use (not the
        // wider numeric(19,6) ExchangeRate.Rate needs).
        builder.Property(x => x.AcquisitionCost).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.SalvageValue).HasColumnType("numeric(19,4)").IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(x => x.RowVersion);
        builder.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
        builder.Ignore(x => x.DomainEvents);
    }
}
