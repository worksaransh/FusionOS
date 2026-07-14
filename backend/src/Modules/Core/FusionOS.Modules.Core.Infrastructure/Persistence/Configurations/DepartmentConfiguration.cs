using FusionOS.Modules.Core.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Core.Infrastructure.Persistence.Configurations;

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).HasMaxLength(200).IsRequired();
        builder.Property(d => d.Code).HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(d => d.RowVersion);
        builder.HasIndex(d => new { d.CompanyId, d.Code });
        builder.Ignore(d => d.DomainEvents);
    }
}
