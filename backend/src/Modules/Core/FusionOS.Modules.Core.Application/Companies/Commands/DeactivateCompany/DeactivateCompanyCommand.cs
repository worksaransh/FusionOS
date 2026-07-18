using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Companies.Contracts;

namespace FusionOS.Modules.Core.Application.Companies.Commands.DeactivateCompany;

/// <summary>
/// Deactivates the caller's own company (Phase I, 2026-07-14 sprint). No
/// CompanyId property for the same reason as UpdateCompanyCommand — Company
/// is the tenant root, so the handler checks tenancy directly.
/// </summary>
public sealed record DeactivateCompanyCommand(Guid Id) : ICommand<CompanyDto?>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.company.deactivate" };
    public string EntityType => "Company";
    public Guid EntityId => Id;
    public string Action => "Deactivated";
}
