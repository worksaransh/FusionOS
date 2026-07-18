using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Companies.Contracts;

namespace FusionOS.Modules.Core.Application.Companies.Queries.GetCompanyById;

/// <summary>
/// Read-gated (Phase I, 2026-07-14 sprint) — requires "core.company.read".
/// A Company is the tenant root itself, not a record *within* a tenant (same
/// reasoning as ListCompaniesQueryHandler), so this deliberately carries no
/// CompanyId property for TenantIsolationBehavior to key off of. The handler
/// enforces "only the caller's own company" directly instead, returning null
/// (mapped to 404 by the controller) both when the id doesn't exist and when
/// it belongs to a different company — the same response either way so a
/// caller can't use this endpoint to probe which company ids exist.
/// </summary>
public sealed record GetCompanyByIdQuery(Guid Id) : IQuery<CompanyDto?>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.company.read" };
}
