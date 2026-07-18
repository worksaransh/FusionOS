using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;

namespace FusionOS.Modules.Hrms.Application.Employees.Queries.ListEmployees;

public sealed record ListEmployeesQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<EmployeeDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "hrms.employee.read" };
}
