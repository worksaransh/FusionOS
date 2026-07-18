using FusionOS.Modules.Core.Application.AuditLog.Contracts;
using FusionOS.Modules.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Core.Infrastructure.Repositories;

/// <summary>Read-only access to the insert-only audit_logs table (Phase H4, 2026-07-14 sprint).</summary>
public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly CoreDbContext _context;

    public AuditLogRepository(CoreDbContext context) => _context = context;

    public async Task<IReadOnlyList<AuditLogEntryDto>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await BuildQuery(companyId, search)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    // Matches on EntityType or Action — the two fields an audit-log search box would reasonably type into.
    private IQueryable<Domain.Audit.AuditLog> Filtered(Guid companyId, string? search)
    {
        var query = _context.AuditLogs.Where(a => a.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(a => EF.Functions.ILike(a.EntityType, pattern) || EF.Functions.ILike(a.Action, pattern));
        }
        return query;
    }

    private IQueryable<AuditLogEntryDto> BuildQuery(Guid companyId, string? search) =>
        from a in Filtered(companyId, search)
        join u in _context.Users on a.ActorId equals u.Id into actorJoin
        from actor in actorJoin.DefaultIfEmpty()
        orderby a.OccurredAt descending
        select new AuditLogEntryDto(
            a.Id, a.EntityType, a.EntityId, a.Action, a.ActorId, actor != null ? actor.Email : null,
            a.CompanyId, a.BranchId, a.OccurredAt, a.CorrelationId);
}
