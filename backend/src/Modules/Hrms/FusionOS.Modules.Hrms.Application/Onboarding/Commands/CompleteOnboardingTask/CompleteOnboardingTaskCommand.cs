using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;

namespace FusionOS.Modules.Hrms.Application.Onboarding.Commands.CompleteOnboardingTask;

public sealed record CompleteOnboardingTaskCommand(Guid CompanyId, Guid WorkflowId, Guid TaskId, string? Notes = null)
    : ICommand<EmployeeWorkflowDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "hrms.onboarding.complete-task" };
    public string EntityType => "EmployeeWorkflow";
    public Guid EntityId => WorkflowId;
    public string Action => "TaskCompleted";
}
