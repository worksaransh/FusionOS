using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;

namespace FusionOS.Modules.Hrms.Application.Onboarding.Queries.GetOnboardingWorkflow;

public sealed record GetOnboardingWorkflowQuery(Guid CompanyId, Guid WorkflowId) : IQuery<EmployeeWorkflowDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "hrms.onboarding.read" };
}
