using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Departments.Contracts;

namespace FusionOS.Modules.Core.Application.Departments.Commands.UpdateDepartment;

/// <summary>Update deliberately excludes Code — it's the immutable business key, same convention as UpdateBranchCommand/UpdateCostCenterCommand.</summary>
public sealed record UpdateDepartmentCommand(Guid CompanyId, Guid DepartmentId, string Name, Guid? BranchId, Guid? ParentDepartmentId)
    : ICommand<DepartmentDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.department.update" };
    public string EntityType => nameof(Domain.Organizations.Department);
    public Guid EntityId => DepartmentId;
    public string Action => "Updated";
}
