using FusionOS.Modules.Core.Domain.Organizations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Core.Infrastructure.Persistence.Configurations;

public sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("branches");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).HasMaxLength(200).IsRequired();
        builder.Property(b => b.Code).HasMaxLength(20).IsRequired();
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(b => b.RowVersion);
        builder.HasIndex(b => new { b.CompanyId, b.Code }).IsUnique();
        builder.Ignore(b => b.DomainEvents);
    }
}
