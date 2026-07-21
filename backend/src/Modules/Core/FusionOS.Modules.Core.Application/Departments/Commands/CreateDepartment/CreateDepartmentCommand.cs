using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Departments.Contracts;

namespace FusionOS.Modules.Core.Application.Departments.Commands.CreateDepartment;

public sealed record CreateDepartmentCommand(Guid CompanyId, Guid? BranchId, string Name, string Code, Guid? ParentDepartmentId = null)
    : ICommand<DepartmentDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.department.create" };
    public string EntityType => nameof(Domain.Organizations.Department);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
