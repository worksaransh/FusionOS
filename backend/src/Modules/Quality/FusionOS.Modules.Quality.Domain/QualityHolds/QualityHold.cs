using FusionOS.SharedKernel;

namespace FusionOS.Modules.Quality.Domain.QualityHolds;

/// <summary>
/// Phase 5 — Quality: a hold/quarantine placed on some other module's record — e.g. an
/// Inventory Lot, a Purchase Order, or a Sales Order — pending investigation, typically
/// (but not necessarily) because an <see cref="Inspections.Inspection"/> failed.
///
/// <see cref="EntityType"/>/<see cref="EntityId"/> is the same opaque polymorphic-reference
/// convention as Core's <c>ApprovalRequest</c> (FusionOS.Modules.Core.Domain.Workflow) —
/// this aggregate doesn't know or care what an "InventoryLot" or "SalesOrder" is, it only
/// tracks a hold against some (EntityType, EntityId) pair, never existence-validated and
/// never a real foreign key.
///
/// <see cref="InspectionId"/> is optional and, when set, a same-module reference to the
/// Inspection that triggered this hold (see RecordInspectionResultsCommand's optional
/// RaiseHoldOnFailure flag) — but a hold can equally be raised standalone with no inspection
/// behind it at all (e.g. a manual quarantine pending investigation).
///
/// Deliberately NOT auto-created by every failed inspection — same restraint as
/// InspectionCompleted's doc comment: quarantining stock or flagging an order reaches into
/// another module's data with no consumer wired to react to it yet, so it stays an explicit,
/// opt-in action (a caller-supplied flag), never an automatic side effect of RecordResults.
/// </summary>
public sealed class QualityHold : TenantAggregateRoot
{
    public string EntityType { get; private set; } = default!;
    public Guid EntityId { get; private set; }
    public string Reason { get; private set; } = default!;
    public Guid? InspectionId { get; private set; }
    public QualityHoldStatus Status { get; private set; }
    public DateTimeOffset? ReleasedAt { get; private set; }
    public string? ReleasedByNote { get; private set; }

    private QualityHold() { }

    public static QualityHold Create(Guid companyId, string entityType, Guid entityId, string reason, Guid? inspectionId)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required.", nameof(entityType));
        if (entityId == Guid.Empty)
            throw new ArgumentException("Entity id is required.", nameof(entityId));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A reason for the hold is required.", nameof(reason));
        if (inspectionId == Guid.Empty)
            throw new ArgumentException("Inspection id, when supplied, cannot be empty — omit it entirely for a hold with no triggering inspection.", nameof(inspectionId));

        return new QualityHold
        {
            CompanyId = companyId,
            EntityType = entityType.Trim(),
            EntityId = entityId,
            Reason = reason.Trim(),
            InspectionId = inspectionId,
            Status = QualityHoldStatus.Active,
        };
    }

    /// <summary>Releases the hold. Only an Active hold can be released; throws if it already was.</summary>
    public void Release(string? note)
    {
        if (Status != QualityHoldStatus.Active)
            throw new InvalidOperationException($"This quality hold is already {Status} — it cannot be released again.");

        Status = QualityHoldStatus.Released;
        ReleasedAt = DateTimeOffset.UtcNow;
        ReleasedByNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }
}
