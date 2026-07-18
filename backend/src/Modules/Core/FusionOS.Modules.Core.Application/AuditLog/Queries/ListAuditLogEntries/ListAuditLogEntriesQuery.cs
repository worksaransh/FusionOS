using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.AuditLog.Contracts;

namespace FusionOS.Modules.Core.Application.AuditLog.Queries.ListAuditLogEntries;

/// <summary>
/// Read side of the insert-only audit trail (Phase H4, 2026-07-14 sprint).
/// Company-scoped like every other List query — TenantIsolationBehavior
/// enforces CompanyId matches the caller's own via reflection, and
/// "core.audit.read" gates it on top. Search added in Phase M5 (2026-07-15 —
/// Search completion): matches on EntityType or Action.
/// </summary>
public sealed record ListAuditLogEntriesQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<AuditLogEntryDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.audit.read" };
}
