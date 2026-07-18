namespace FusionOS.Modules.Core.Application.AuditLog.Contracts;

/// <summary>
/// Published read-side DTO for the insert-only audit_logs table
/// (04_DATABASE_GUIDELINES.md §5, Phase H4 2026-07-14 sprint). ActorEmail is
/// resolved via a join at read time — the write side (AuditLogEntry/AuditLog)
/// only ever stores ActorId.
/// </summary>
public sealed record AuditLogEntryDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Action,
    Guid ActorId,
    string? ActorEmail,
    Guid CompanyId,
    Guid? BranchId,
    DateTimeOffset OccurredAt,
    string CorrelationId);
