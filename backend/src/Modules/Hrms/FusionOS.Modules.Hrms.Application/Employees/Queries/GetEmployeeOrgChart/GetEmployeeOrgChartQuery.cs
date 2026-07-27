using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;

namespace FusionOS.Modules.Hrms.Application.Employees.Queries.GetEmployeeOrgChart;

public sealed record GetEmployeeOrgChartQuery(Guid CompanyId, Guid EmployeeId) : IQuery<EmployeeOrgChartDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "hrms.employee.read" };
}
