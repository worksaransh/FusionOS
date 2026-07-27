using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;

namespace FusionOS.Modules.Hrms.Application.Employees.Commands.UpdateEmployee;

/// <summary>
/// Employee had no Update command at all before this org-chart/Department depth pass (2026-07-23)
/// — only Create/Deactivate. Updates FullName/Email/DepartmentId/ManagerId; Code, DepartmentName,
/// and HireDate stay immutable via this path (see Employee.UpdateDetails's own doc comment).
/// </summary>
public sealed record UpdateEmployeeCommand(Guid CompanyId, Guid EmployeeId, string FullName, string Email, Guid? DepartmentId, Guid? ManagerId)
    : ICommand<EmployeeDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "hrms.employee.update" };
    public string EntityType => nameof(Domain.Employees.Employee);
    public Guid EntityId => EmployeeId;
    public string Action => "Updated";
}
