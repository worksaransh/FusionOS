namespace FusionOS.Modules.IntegrationHub.Domain.Migrations;

/// <summary>Where a <see cref="MigrationStagingRecord"/> sits in the Mapping/Transform → Validation → Load pipeline (11_SAP_MIGRATION.md §2).</summary>
public enum MappingStatus
{
    Pending,
    Mapped,
    Failed,
}
