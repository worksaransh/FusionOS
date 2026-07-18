namespace FusionOS.Modules.Core.Application.AuditLog.Contracts;

/// <summary>Read-only access to the insert-only audit trail (Phase H4, 2026-07-14 sprint).</summary>
public interface IAuditLogRepository
{
    /// <summary>Search (Phase M5, 2026-07-15) matches on EntityType or Action.</summary>
    Task<IReadOnlyList<AuditLogEntryDto>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
