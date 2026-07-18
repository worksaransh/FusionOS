using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;

namespace FusionOS.Modules.Hrms.Application.Employees.Commands.CreateEmployee;

public sealed record CreateEmployeeCommand(Guid CompanyId, string Code, string FullName, string Email, string? DepartmentName, DateTimeOffset HireDate)
    : ICommand<EmployeeDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "hrms.employee.create" };
    public string EntityType => nameof(Domain.Employees.Employee);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
