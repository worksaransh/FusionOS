using FusionOS.SharedKernel;

namespace FusionOS.Modules.IntegrationHub.Domain.Migrations;

/// <summary>
/// One raw record landed by an extractor into the `migration.*` staging
/// schema (11_SAP_MIGRATION.md §2) — "holding raw extracted data as-is,
/// before any transformation, preserves an audit trail of exactly what was
/// read from the source." Deliberately a SEPARATE aggregate from
/// <see cref="MigrationJob"/> (not an owned collection on it): a single
/// migration job can stage thousands of records, which would make an
/// EF owned-collection mapping impractical to query/page.
///
/// <see cref="MigrationJobId"/> is a same-module FK, existence-validated in
/// the command handler — same convention as ConnectorConnection.IntegrationConnectorId.
///
/// <see cref="RecordType"/> is the source system's own vocabulary
/// ("LEDGER", "STOCKITEM", "VOUCHER", "GROUP" for Tally) rather than a
/// FusionOS-side enum, since different source systems bring their own
/// record shapes. <see cref="TargetEntityType"/>/<see cref="TargetEntityId"/>
/// are the opaque polymorphic reference set once the record is
/// successfully loaded into a real FusionOS record — same convention as
/// Quality's Inspection.ReferenceId.
/// </summary>
public sealed class MigrationStagingRecord : TenantAggregateRoot
{
    public Guid MigrationJobId { get; private set; }
    public string RecordType { get; private set; } = default!;
    public string RawDataJson { get; private set; } = default!;
    public MappingStatus MappingStatus { get; private set; }

    /// <summary>JSON array of validation failure messages — set by <see cref="MarkValidationFailed"/>, null otherwise.</summary>
    public string? ValidationErrors { get; private set; }

    public string? TargetEntityType { get; private set; }
    public Guid? TargetEntityId { get; private set; }

    private MigrationStagingRecord() { }

    public static MigrationStagingRecord Stage(Guid companyId, Guid migrationJobId, string recordType, string rawDataJson)
    {
        if (migrationJobId == Guid.Empty)
            throw new ArgumentException("Migration job id is required.", nameof(migrationJobId));
        if (string.IsNullOrWhiteSpace(recordType))
            throw new ArgumentException("Record type is required.", nameof(recordType));
        if (string.IsNullOrWhiteSpace(rawDataJson))
            throw new ArgumentException("Raw data is required.", nameof(rawDataJson));

        return new MigrationStagingRecord
        {
            CompanyId = companyId,
            MigrationJobId = migrationJobId,
            RecordType = recordType.Trim(),
            RawDataJson = rawDataJson,
            MappingStatus = MappingStatus.Pending,
        };
    }

    public void MarkMapped()
    {
        MappingStatus = MappingStatus.Mapped;
        ValidationErrors = null;
    }

    public void MarkValidationFailed(string errorsJson)
    {
        if (string.IsNullOrWhiteSpace(errorsJson))
            throw new ArgumentException("Validation errors are required.", nameof(errorsJson));

        MappingStatus = MappingStatus.Failed;
        ValidationErrors = errorsJson;
    }

    /// <summary>Called once the record has been loaded into a real FusionOS record via a real Application-layer command — never direct SQL (11_SAP_MIGRATION.md §2).</summary>
    public void MarkLoaded(string targetEntityType, Guid targetEntityId)
    {
        if (string.IsNullOrWhiteSpace(targetEntityType))
            throw new ArgumentException("Target entity type is required.", nameof(targetEntityType));
        if (targetEntityId == Guid.Empty)
            throw new ArgumentException("Target entity id is required.", nameof(targetEntityId));

        TargetEntityType = targetEntityType.Trim();
        TargetEntityId = targetEntityId;
    }
}
