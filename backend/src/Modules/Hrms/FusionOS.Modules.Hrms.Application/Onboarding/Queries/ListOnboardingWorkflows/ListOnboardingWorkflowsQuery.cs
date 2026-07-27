using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;

namespace FusionOS.Modules.Hrms.Application.Onboarding.Queries.ListOnboardingWorkflows;

public sealed record ListOnboardingWorkflowsQuery(Guid CompanyId, Guid? EmployeeId = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<EmployeeWorkflowDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "hrms.onboarding.read" };
}
