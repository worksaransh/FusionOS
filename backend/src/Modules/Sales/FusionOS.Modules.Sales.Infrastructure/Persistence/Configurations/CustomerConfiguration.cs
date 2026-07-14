using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Sales.Infrastructure.Persistence.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Domain.Customers.Customer>
{
    public void Configure(EntityTypeBuilder<Domain.Customers.Customer> builder)
    {
        builder.ToTable("customers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.ContactEmail).HasMaxLength(320);
        builder.Property(x => x.CreditLimit).HasColumnType("numeric(19,4)");
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(x => x.RowVersion);
        builder.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
        builder.Ignore(x => x.DomainEvents);
    }
}
