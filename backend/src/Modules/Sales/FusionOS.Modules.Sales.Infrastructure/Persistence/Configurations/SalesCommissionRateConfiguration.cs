using FusionOS.Modules.Sales.Domain.Commissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class SalesCommissionRateConfiguration : IEntityTypeConfiguration<SalesCommissionRate>
{
    public void Configure(EntityTypeBuilder<SalesCommissionRate> builder)
    {
        builder.ToTable("sales_commission_rates");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RatePercentage).HasColumnType("numeric(5,2)").IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);

        // One rate row per (CompanyId, UserId) — enforced by the get-or-create
        // upsert in SetCommissionRateCommandHandler, backed here by a unique
        // index so a race can't produce duplicates.
        builder.HasIndex(x => new { x.CompanyId, x.UserId }).IsUnique();
    }
}
