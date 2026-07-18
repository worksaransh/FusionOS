using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Warehouse.Application.Zones.Contracts;

namespace FusionOS.Modules.Warehouse.Application.Zones.Queries.GetZoneById;

/// <summary>
/// Zones had no GetById route at all (unlike Product/Warehouse's dead stubs) —
/// this adds the first one. Tenant-scoped via the CompanyId property, which
/// TenantIsolationBehavior enforces against the caller's own company; the
/// handler additionally checks the loaded Zone's CompanyId to guard against a
/// cross-tenant lookup by guessed id. Read-gated with the existing
/// "warehouse.zone.read" permission — no new permission code needed here.
/// </summary>
public sealed record GetZoneByIdQuery(Guid CompanyId, Guid Id)
    : IQuery<ZoneDto?>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "warehouse.zone.read" };
}
