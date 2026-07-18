using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;

namespace FusionOS.Modules.Hrms.Application.Employees.Commands.DeactivateEmployee;

public sealed record DeactivateEmployeeCommand(Guid CompanyId, Guid EmployeeId)
    : ICommand<EmployeeDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "hrms.employee.deactivate" };
    public string EntityType => nameof(Domain.Employees.Employee);
    public Guid EntityId => EmployeeId;
    public string Action => "Deactivated";
}
