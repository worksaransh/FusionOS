using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;

namespace FusionOS.Modules.Hrms.Application.Onboarding.Commands.CreateOnboardingWorkflow;

/// <summary>Starts an onboarding workflow for an employee with a caller-supplied checklist. Thin wrapper around the shared EmployeeWorkflow entity (WorkflowKind.Onboarding) — see EmployeeWorkflow.cs's own doc comment for why the entity is shared but this command type is not.</summary>
public sealed record CreateOnboardingWorkflowCommand(Guid CompanyId, Guid EmployeeId, IReadOnlyCollection<string> TaskTitles)
    : ICommand<EmployeeWorkflowDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "hrms.onboarding.create" };
    public string EntityType => "EmployeeWorkflow";
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
