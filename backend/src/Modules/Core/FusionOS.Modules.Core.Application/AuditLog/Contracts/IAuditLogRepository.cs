namespace FusionOS.Modules.Core.Application.AuditLog.Contracts;

/// <summary>Read-only access to the insert-only audit trail (Phase H4, 2026-07-14 sprint).</summary>
public interface IAuditLogRepository
{
    /// <summary>Search (Phase M5, 2026-07-15) matches on EntityType or Action.</summary>
    Task<IReadOnlyList<AuditLogEntryDto>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);

    /// <summary>
    /// Every audit entry for one specific (EntityType, EntityId) pair, oldest
    /// first — added for GetEntityActivityTimelineQuery, which merges this
    /// with ICommentRepository.ListByEntityAsync's comments into one
    /// chronological timeline. Unlike ListAsync (a company-wide, paged,
    /// free-text search over the whole audit log), this is un-paged and
    /// scoped to a single record, since a single entity's history is expected
    /// to be small enough to render in full.
    /// </summary>
    Task<IReadOnlyList<AuditLogEntryDto>> ListByEntityAsync(Guid companyId, string entityType, Guid entityId, CancellationToken cancellationToken = default);
}
