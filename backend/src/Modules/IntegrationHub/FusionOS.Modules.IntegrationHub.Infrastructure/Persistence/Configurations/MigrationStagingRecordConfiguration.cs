using FusionOS.Modules.IntegrationHub.Domain.Migrations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FusionOS.Modules.IntegrationHub.Infrastructure.Persistence.Configurations;

public sealed class MigrationStagingRecordConfiguration : IEntityTypeConfiguration<MigrationStagingRecord>
{
    public void Configure(EntityTypeBuilder<MigrationStagingRecord> builder)
    {
        builder.ToTable("migration_staging_records");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RecordType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.RawDataJson).HasColumnType("jsonb").IsRequired();
        builder.Property(x => x.MappingStatus).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(x => x.ValidationErrors).HasColumnType("jsonb");
        builder.Property(x => x.TargetEntityType).HasMaxLength(100);
        builder.UseXminAsConcurrencyToken(); // Postgres system column, not the app-level RowVersion byte[] (04_DATABASE_GUIDELINES.md)
        builder.Ignore(x => x.RowVersion);
        builder.Ignore(x => x.DomainEvents);
        // No FK constraint to MigrationJob: existence is validated in the command handler,
        // keeping this reference consistent with every other same-module reference in this
        // codebase (e.g. ConnectorConnection.IntegrationConnectorId, BudgetLine.AccountId).
        builder.HasIndex(x => new { x.CompanyId, x.MigrationJobId, x.MappingStatus });
    }
}
