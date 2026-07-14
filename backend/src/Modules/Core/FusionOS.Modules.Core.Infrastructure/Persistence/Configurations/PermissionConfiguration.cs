using FusionOS.Modules.Core.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Core.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Code).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Module).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(300);
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md — SQL Server IsRowVersion() idiom fixed)
        builder.Ignore(p => p.RowVersion);
        builder.HasIndex(p => p.Code).IsUnique();
    }
}
