using FusionOS.Modules.Finance.Domain.ExchangeRates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Finance.Infrastructure.Persistence.Configurations;

public sealed class ExchangeRateConfiguration : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.ToTable("exchange_rates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FromCurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.ToCurrencyCode).HasMaxLength(3).IsRequired();
        // numeric(19,6), not (19,4) like BankStatementLine.Amount — an FX rate
        // routinely needs more than 4 decimal places (e.g. JPY-denominated pairs).
        builder.Property(x => x.Rate).HasColumnType("numeric(19,6)").IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(x => x.RowVersion);
        // Multiple dated rates are allowed for the same pair, but not two for the
        // same pair on the same date (see ExchangeRate.cs's class doc comment).
        builder.HasIndex(x => new { x.CompanyId, x.FromCurrencyCode, x.ToCurrencyCode, x.EffectiveDate }).IsUnique();
        builder.Ignore(x => x.DomainEvents);
    }
}
