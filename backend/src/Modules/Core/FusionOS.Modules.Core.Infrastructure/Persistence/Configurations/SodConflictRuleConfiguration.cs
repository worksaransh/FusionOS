using FusionOS.Modules.Core.Domain.SodConflictRules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.Core.Infrastructure.Persistence.Configurations;

public sealed class SodConflictRuleConfiguration : IEntityTypeConfiguration<SodConflictRule>
{
    public void Configure(EntityTypeBuilder<SodConflictRule> builder)
    {
        builder.ToTable("sod_conflict_rules");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ConflictingPermissionA).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ConflictingPermissionB).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.ConflictingPermissionA, x.ConflictingPermissionB }).IsUnique();
    }
}
