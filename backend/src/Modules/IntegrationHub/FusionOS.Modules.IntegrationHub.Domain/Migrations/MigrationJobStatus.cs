namespace FusionOS.Modules.IntegrationHub.Domain.Migrations;

/// <summary>
/// Pipeline stage of a <see cref="MigrationJob"/>, per 11_SAP_MIGRATION.md §2:
/// Extractor → Staging Schema → Mapping/Transform → Validation → FusionOS Load → Reconciliation.
/// </summary>
public enum MigrationJobStatus
{
    Created,
    Extracting,
    Extracted,
    Validating,
    Validated,
    Loading,
    Completed,
    Failed,
}
