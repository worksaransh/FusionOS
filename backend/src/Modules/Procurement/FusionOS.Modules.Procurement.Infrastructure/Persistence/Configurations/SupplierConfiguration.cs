using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Procurement.Infrastructure.Persistence.Configurations;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Domain.Suppliers.Supplier>
{
    public void Configure(EntityTypeBuilder<Domain.Suppliers.Supplier> builder)
    {
        builder.ToTable("suppliers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(30).IsRequired();
        builder.Property(x => x.ContactEmail).HasMaxLength(320);
        builder.Property(x => x.ContactPhone).HasMaxLength(30);
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(x => x.RowVersion);
        builder.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
        builder.Ignore(x => x.DomainEvents);
    }
}
