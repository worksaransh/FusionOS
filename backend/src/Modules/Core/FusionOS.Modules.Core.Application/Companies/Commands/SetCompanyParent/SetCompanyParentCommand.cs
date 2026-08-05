using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Companies.Contracts;

namespace FusionOS.Modules.Core.Application.Companies.Commands.SetCompanyParent;

/// <summary>
/// Sets or clears the caller's own company's parent company for consolidation
/// purposes (Phase 2.4 — multi-entity/subsidiary consolidation). Pass null for
/// ParentCompanyId to detach from any parent. Like UpdateCompanyCommand,
/// deliberately has no separate CompanyId property — Company is the tenant
/// root, so the handler checks "Id must equal the caller's own CompanyId"
/// directly, same as UpdateCompanyCommand/DeactivateCompanyCommand. Gated on
/// the same core.company.update permission as UpdateCompanyCommand — this is
/// still just "update this company's own details," one more field on Company.
/// </summary>
public sealed record SetCompanyParentCommand(Guid Id, Guid? ParentCompanyId)
    : ICommand<CompanyDto?>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.company.update" };
    public string EntityType => "Company";
    public Guid EntityId => Id;
    public string Action => "ParentChanged";
}
