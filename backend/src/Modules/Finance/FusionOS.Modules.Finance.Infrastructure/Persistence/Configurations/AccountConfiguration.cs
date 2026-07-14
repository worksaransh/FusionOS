using FusionOS.Modules.Finance.Domain.Accounts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Finance.Infrastructure.Persistence.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(20).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.AccountType).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(x => x.RowVersion);
        builder.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
        builder.Ignore(x => x.DomainEvents);

        // Self-referencing hierarchy (same module, real FK) — no navigation property
        // exposed on the aggregate itself, so this is configured as a shadow FK
        // relationship rather than a Parent/Children navigation.
        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(x => x.ParentAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
