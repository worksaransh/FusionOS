using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;

namespace FusionOS.Modules.Hrms.Application.Offboarding.Queries.ListOffboardingWorkflows;

public sealed record ListOffboardingWorkflowsQuery(Guid CompanyId, Guid? EmployeeId = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<EmployeeWorkflowDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "hrms.offboarding.read" };
}
