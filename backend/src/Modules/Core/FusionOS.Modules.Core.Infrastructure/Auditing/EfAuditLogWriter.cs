using FusionOS.Modules.Core.Domain.Audit;
using FusionOS.Modules.Core.Infrastructure.Persistence;
using FusionOS.SharedKernel.Auditing;

namespace FusionOS.Modules.Core.Infrastructure.Auditing;

/// <summary>
/// The Core module's concrete implementation of the platform-wide IAuditLogWriter
/// contract (04_DATABASE_GUIDELINES.md §5). Registered once at the Host so every
/// module can write audit entries without referencing Core directly.
/// </summary>
public sealed class EfAuditLogWriter : IAuditLogWriter
{
    private readonly CoreDbContext _context;

    public EfAuditLogWriter(CoreDbContext context) => _context = context;

    public async Task WriteAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        _context.AuditLogs.Add(AuditLog.FromEntry(
            entry.Id, entry.EntityType, entry.EntityId, entry.Action, entry.ActorId,
            entry.CompanyId, entry.BranchId, entry.OccurredAt, entry.ChangesJson, entry.CorrelationId));

        await _context.SaveChangesAsync(cancellationToken);
    }
}
