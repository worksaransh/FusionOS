using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Departments.Contracts;

namespace FusionOS.Modules.Core.Application.Departments.Commands.DeactivateDepartment;

/// <summary>Soft-deactivate only — never a real delete, same convention as DeactivateBranchCommand.</summary>
public sealed record DeactivateDepartmentCommand(Guid CompanyId, Guid DepartmentId)
    : ICommand<DepartmentDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.department.deactivate" };
    public string EntityType => nameof(Domain.Organizations.Department);
    public Guid EntityId => DepartmentId;
    public string Action => "Deactivated";
}
