namespace FusionOS.Modules.Core.Domain.Audit;

/// <summary>
/// Concrete storage for FusionOS.SharedKernel.Auditing.AuditLogEntry — insert-only,
/// never updated or deleted (04_DATABASE_GUIDELINES.md §5).
/// </summary>
public sealed class AuditLog
{
    public Guid Id { get; private set; }
    public string EntityType { get; private set; } = default!;
    public Guid EntityId { get; private set; }
    public string Action { get; private set; } = default!;
    public Guid ActorId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid? BranchId { get; private set; }
    public DateTimeOffset OccurredAt { get; private set; }
    public string? ChangesJson { get; private set; }
    public string CorrelationId { get; private set; } = default!;

    private AuditLog() { }

    public static AuditLog FromEntry(Guid id, string entityType, Guid entityId, string action, Guid actorId,
        Guid companyId, Guid? branchId, DateTimeOffset occurredAt, string? changesJson, string correlationId) => new()
    {
        Id = id,
        EntityType = entityType,
        EntityId = entityId,
        Action = action,
        ActorId = actorId,
        CompanyId = companyId,
        BranchId = branchId,
        OccurredAt = occurredAt,
        ChangesJson = changesJson,
        CorrelationId = correlationId,
    };
}
