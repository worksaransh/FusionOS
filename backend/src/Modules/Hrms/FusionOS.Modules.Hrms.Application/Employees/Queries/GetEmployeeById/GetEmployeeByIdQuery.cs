using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;

namespace FusionOS.Modules.Hrms.Application.Employees.Queries.GetEmployeeById;

public sealed record GetEmployeeByIdQuery(Guid CompanyId, Guid EmployeeId) : IQuery<EmployeeDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "hrms.employee.read" };
}
