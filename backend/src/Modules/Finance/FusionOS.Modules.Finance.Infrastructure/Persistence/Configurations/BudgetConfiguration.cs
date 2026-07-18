using FusionOS.Modules.Finance.Domain.Budgets;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Finance.Infrastructure.Persistence.Configurations;

public sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("budgets");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(x => x.RowVersion);
        // No unique index — Budget has no business-key field the way
        // Account/CostCenter/TaxJurisdiction have Code; two budgets with the
        // same Name/period are a legitimate (if unusual) data-entry case, not
        // a duplicate to reject.
        builder.HasIndex(x => new { x.CompanyId, x.PeriodStart, x.PeriodEnd });
        builder.Ignore(x => x.DomainEvents);
    }
}
