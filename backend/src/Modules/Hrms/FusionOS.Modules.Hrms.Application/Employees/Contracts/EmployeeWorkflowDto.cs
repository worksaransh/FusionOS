namespace FusionOS.Modules.Hrms.Application.Employees.Contracts;

public sealed record WorkflowTaskDto(Guid Id, string Title, bool IsCompleted, DateTimeOffset? CompletedAt, string? Notes);

/// <summary>
/// Shared DTO for both Onboarding and Offboarding workflows — backs both Onboarding/* and
/// Offboarding/* commands/queries, since both directions are the same EmployeeWorkflow entity
/// (see EmployeeWorkflow.cs's own doc comment for why). Kind distinguishes which direction a
/// given instance represents ("Onboarding" or "Offboarding").
/// </summary>
public sealed record EmployeeWorkflowDto(
    Guid Id,
    Guid EmployeeId,
    string Kind,
    string Status,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    IReadOnlyList<WorkflowTaskDto> Tasks);

/// <summary>Single place that turns an EmployeeWorkflow aggregate into its DTO, shared by every Onboarding/Offboarding handler that returns one.</summary>
public static class EmployeeWorkflowMapper
{
    public static EmployeeWorkflowDto ToDto(Domain.Onboarding.EmployeeWorkflow workflow) =>
        new(
            workflow.Id,
            workflow.EmployeeId,
            workflow.Kind.ToString(),
            workflow.Status.ToString(),
            workflow.StartedAt,
            workflow.CompletedAt,
            workflow.Tasks.Select(t => new WorkflowTaskDto(t.Id, t.Title, t.IsCompleted, t.CompletedAt, t.Notes)).ToList());
}
