using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;

namespace FusionOS.Modules.Hrms.Application.Offboarding.Queries.GetOffboardingWorkflow;

public sealed record GetOffboardingWorkflowQuery(Guid CompanyId, Guid WorkflowId) : IQuery<EmployeeWorkflowDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "hrms.offboarding.read" };
}
