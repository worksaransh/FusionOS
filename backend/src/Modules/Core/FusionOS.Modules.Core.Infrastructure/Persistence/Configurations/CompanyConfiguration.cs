using FusionOS.Modules.Core.Domain.Companies;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Core.Infrastructure.Persistence.Configurations;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.LegalName).HasMaxLength(200).IsRequired();
        builder.Property(c => c.TaxId).HasMaxLength(50);
        builder.Property(c => c.BaseCurrency).HasMaxLength(3).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(c => c.RowVersion);
        builder.HasIndex(c => c.Name);
        builder.Ignore(c => c.DomainEvents);
    }
}
