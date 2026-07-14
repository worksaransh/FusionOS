namespace FusionOS.SharedKernel.Auditing;

/// <summary>
/// Platform-wide audit record shape, per 04_DATABASE_GUIDELINES.md §5. The concrete
/// storage entity/table lives in the Core module; every other module depends only
/// on <see cref="IAuditLogWriter"/>, never on Core's Infrastructure layer directly.
/// </summary>
public sealed record AuditLogEntry(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Action,
    Guid ActorId,
    Guid CompanyId,
    Guid? BranchId,
    DateTimeOffset OccurredAt,
    string? ChangesJson,
    string CorrelationId);
