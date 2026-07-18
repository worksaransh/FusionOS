using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Companies.Contracts;

namespace FusionOS.Modules.Core.Application.Companies.Commands.UpdateCompany;

/// <summary>
/// Renames/updates the caller's own company (Phase I, 2026-07-14 sprint).
/// Like GetCompanyByIdQuery, deliberately has no CompanyId property — Company
/// is the tenant root, so TenantIsolationBehavior can't key off it; the
/// handler checks "Id must equal the caller's own CompanyId" directly.
/// </summary>
public sealed record UpdateCompanyCommand(Guid Id, string Name, string LegalName, string? TaxId)
    : ICommand<CompanyDto?>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.company.update" };
    public string EntityType => "Company";
    public Guid EntityId => Id;
    public string Action => "Updated";
}
