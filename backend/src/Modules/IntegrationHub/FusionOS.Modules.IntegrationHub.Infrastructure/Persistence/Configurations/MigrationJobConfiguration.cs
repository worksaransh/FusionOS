using FusionOS.Modules.IntegrationHub.Domain.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.IntegrationHub.Infrastructure.Persistence.Configurations;

public sealed class MigrationJobConfiguration : IEntityTypeConfiguration<MigrationJob>
{
    public void Configure(EntityTypeBuilder<MigrationJob> builder)
    {
        builder.ToTable("migration_jobs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SourceSystem).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.ReconciliationNote).HasMaxLength(2000);
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => new { x.CompanyId, x.Status });
    }
}
