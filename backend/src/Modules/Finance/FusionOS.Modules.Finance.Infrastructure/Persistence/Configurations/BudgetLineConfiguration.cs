using FusionOS.Modules.Finance.Domain.BudgetLines;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Finance.Infrastructure.Persistence.Configurations;

/// <summary>
/// No unique index on (CompanyId, BudgetId, AccountId, CostCenterId) — a
/// natural-seeming constraint given CostCenterId is nullable, but a
/// nullable-column unique index isn't an established pattern anywhere else
/// in this codebase (every existing unique index — Account/CostCenter/
/// BankAccount/TaxJurisdiction's (CompanyId, Code), TaxRate's (CompanyId,
/// TaxJurisdictionId, Code), ExchangeRate's four-column tuple — is over
/// non-nullable columns only). Postgres treats NULL as distinct from every
/// other NULL in a unique index, so "one line per account per cost center"
/// wouldn't even be enforced correctly by a naive unique index once
/// CostCenterId is NULL for two lines on the same account. Rather than
/// invent a partial-index workaround this slice wasn't asked to design,
/// duplicate (Account, CostCenter) budget lines on the same Budget are
/// simply allowed — a real-world duplicate is a user mistake to notice and
/// correct via UpdateAmount, not something the schema needs to prevent.
/// </summary>
public sealed class BudgetLineConfiguration : IEntityTypeConfiguration<BudgetLine>
{
    public void Configure(EntityTypeBuilder<BudgetLine> builder)
    {
        builder.ToTable("budget_lines");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.BudgetedAmount).HasColumnType("numeric(19,4)").IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(1000);
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(x => x.RowVersion);
        builder.HasIndex(x => new { x.CompanyId, x.BudgetId });
        builder.Ignore(x => x.DomainEvents);
    }
}
