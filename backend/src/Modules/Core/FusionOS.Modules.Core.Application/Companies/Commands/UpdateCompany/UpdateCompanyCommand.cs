using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Companies.Contracts;

namespace FusionOS.Modules.Core.Application.Companies.Commands.UpdateCompany;

/// <summary>
/// Renames/updates the caller's own company (Phase I, 2026-07-14 sprint).
/// Like GetCompanyByIdQuery, deliberately has no CompanyId property — Company
/// is the tenant root, so TenantIsolationBehavior can't key off it; the
/// handler checks "Id must equal the caller's own CompanyId" directly.
///
/// Deliberately does NOT carry ParentCompanyId (Phase 2.4 — multi-entity/
/// subsidiary consolidation): folding an "unset means leave unchanged, but
/// this type's default null also means detach" field into a record with a
/// default parameter would silently clear an existing parent link for every
/// pre-existing caller that doesn't know about the new field. Setting/changing
/// a company's parent is a separate, explicit action — see
/// SetCompanyParentCommand — precisely so this command's existing behavior
/// (and every existing caller of it) is undisturbed.
/// </summary>
public sealed record UpdateCompanyCommand(Guid Id, string Name, string LegalName, string? TaxId)
    : ICommand<CompanyDto?>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.company.update" };
    public string EntityType => "Company";
    public Guid EntityId => Id;
    public string Action => "Updated";
}
