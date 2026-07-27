using FusionOS.Modules.Finance.Domain.AccountingPeriods;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Finance.Infrastructure.Persistence.Configurations;

public sealed class AccountingPeriodConfiguration : IEntityTypeConfiguration<AccountingPeriod>
{
    public void Configure(EntityTypeBuilder<AccountingPeriod> builder)
    {
        builder.ToTable("accounting_periods");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        // Not a unique index: overlap is a range comparison (OverlapsWith), not an equality
        // constraint a DB unique index could express — enforced in CreateAccountingPeriodCommandHandler.
        builder.HasIndex(x => new { x.CompanyId, x.PeriodStart, x.PeriodEnd });
        builder.HasIndex(x => new { x.CompanyId, x.Status });
    }
}
