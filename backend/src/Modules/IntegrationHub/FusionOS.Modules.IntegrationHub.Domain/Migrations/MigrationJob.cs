using FusionOS.SharedKernel;
using FusionOS.Modules.IntegrationHub.Domain.Migrations.Events;

namespace FusionOS.Modules.IntegrationHub.Domain.Migrations;

/// <summary>
/// Phase 8/9 slice — the legacy-system migration pipeline described in
/// 11_SAP_MIGRATION.md §2: Source System → Extractor → Staging Schema →
/// Mapping/Transform → Validation → FusionOS Load → Reconciliation. One
/// MigrationJob tracks a single migration run for one source system for
/// one company (a customer may run several over their engagement — trial
/// migration, then cutover — each is its own job, per §3's phasing).
///
/// <see cref="SourceSystem"/> is free text ("Tally", "Busy", "Odoo", "CSV",
/// ...), not an enum: the blueprint doc lists many current and future
/// source systems and hardcoding an enum now would be premature — same
/// reasoning as Ai's Recommendation.Type being free-text.
///
/// This is intentionally NOT the same thing as IntegrationHub's connector
/// sync framework (IConnectorSyncAdapter/ConnectorConnection/
/// ConnectorSyncRun): that is ongoing incremental sync against a live,
/// still-connected system; this is a one-time (or few-times, during a
/// parallel-run period per §3.4) bulk import from a system being retired.
/// </summary>
public sealed class MigrationJob : TenantAggregateRoot
{
    public string SourceSystem { get; private set; } = default!;
    public MigrationJobStatus Status { get; private set; }
    public int TotalRecordsExtracted { get; private set; }
    public int TotalRecordsLoaded { get; private set; }
    public int TotalRecordsFailed { get; private set; }

    /// <summary>The honest, human-readable reconciliation summary — §6's success criterion (do totals match within tolerance). Null until <see cref="Reconcile"/> runs.</summary>
    public string? ReconciliationNote { get; private set; }

    private MigrationJob() { }

    public static MigrationJob Create(Guid companyId, string sourceSystem)
    {
        if (string.IsNullOrWhiteSpace(sourceSystem))
            throw new ArgumentException("Source system is required.", nameof(sourceSystem));

        var job = new MigrationJob
        {
            CompanyId = companyId,
            SourceSystem = sourceSystem.Trim(),
            Status = MigrationJobStatus.Created,
        };

        job.Raise(new MigrationJobCreated(job.Id, companyId, job.SourceSystem));
        return job;
    }

    public void StartExtraction()
    {
        if (Status != MigrationJobStatus.Created)
            throw new InvalidOperationException($"Cannot start extraction from status '{Status}'.");

        Status = MigrationJobStatus.Extracting;
    }

    public void CompleteExtraction(int recordCount)
    {
        if (Status != MigrationJobStatus.Extracting)
            throw new InvalidOperationException($"Cannot complete extraction from status '{Status}'.");
        if (recordCount < 0)
            throw new ArgumentOutOfRangeException(nameof(recordCount), "Record count cannot be negative.");

        TotalRecordsExtracted = recordCount;
        Status = MigrationJobStatus.Extracted;
    }

    public void StartValidation()
    {
        if (Status != MigrationJobStatus.Extracted)
            throw new InvalidOperationException($"Cannot start validation from status '{Status}'.");

        Status = MigrationJobStatus.Validating;
    }

    public void CompleteValidation()
    {
        if (Status != MigrationJobStatus.Validating)
            throw new InvalidOperationException($"Cannot complete validation from status '{Status}'.");

        Status = MigrationJobStatus.Validated;
    }

    public void StartLoad()
    {
        if (Status != MigrationJobStatus.Validated)
            throw new InvalidOperationException($"Cannot start load from status '{Status}'.");

        Status = MigrationJobStatus.Loading;
    }

    public void CompleteLoad(int loadedCount, int failedCount)
    {
        if (Status != MigrationJobStatus.Loading)
            throw new InvalidOperationException($"Cannot complete load from status '{Status}'.");
        if (loadedCount < 0)
            throw new ArgumentOutOfRangeException(nameof(loadedCount), "Loaded count cannot be negative.");
        if (failedCount < 0)
            throw new ArgumentOutOfRangeException(nameof(failedCount), "Failed count cannot be negative.");

        TotalRecordsLoaded = loadedCount;
        TotalRecordsFailed = failedCount;
        Status = MigrationJobStatus.Completed;
    }

    /// <summary>Records the §6 reconciliation summary. Only meaningful once loading has finished.</summary>
    public void Reconcile(string note)
    {
        if (Status != MigrationJobStatus.Completed)
            throw new InvalidOperationException($"Cannot reconcile from status '{Status}' — the load stage must complete first.");
        if (string.IsNullOrWhiteSpace(note))
            throw new ArgumentException("A reconciliation note is required.", nameof(note));

        ReconciliationNote = note.Trim();
    }

    /// <summary>Terminal failure from any non-terminal state — a bad extract, an unparseable export file, etc.</summary>
    public void Fail(string reason)
    {
        if (Status is MigrationJobStatus.Completed or MigrationJobStatus.Failed)
            throw new InvalidOperationException($"Cannot fail a job already in terminal status '{Status}'.");
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A failure reason is required.", nameof(reason));

        ReconciliationNote = reason.Trim();
        Status = MigrationJobStatus.Failed;
    }
}
