using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;

namespace FusionOS.Modules.Hrms.Application.Offboarding.Commands.CreateOffboardingWorkflow;

/// <summary>
/// Starts an offboarding workflow for a departing employee with a caller-supplied checklist. Thin
/// wrapper around the shared EmployeeWorkflow entity (WorkflowKind.Offboarding) — see
/// EmployeeWorkflow.cs's own doc comment for why the entity is shared but this command type is
/// not. Deliberately does NOT deactivate the employee itself — see EmployeeWorkflow.cs's class
/// doc comment ("Offboarding and Employee.Deactivate() are deliberately NOT coupled"). Deactivate
/// the employee separately via DeactivateEmployeeCommand.
/// </summary>
public sealed record CreateOffboardingWorkflowCommand(Guid CompanyId, Guid EmployeeId, IReadOnlyCollection<string> TaskTitles)
    : ICommand<EmployeeWorkflowDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "hrms.offboarding.create" };
    public string EntityType => "EmployeeWorkflow";
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
