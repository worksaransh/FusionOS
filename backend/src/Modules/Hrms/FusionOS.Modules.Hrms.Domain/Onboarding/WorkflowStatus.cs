namespace FusionOS.Modules.Hrms.Domain.Onboarding;

/// <summary>InProgress -> Completed, one-way. Completed is reached automatically once every task is done (see EmployeeWorkflow.CompleteTask's own doc comment) — there is no separate explicit "close the workflow" action in this slice.</summary>
public enum WorkflowStatus
{
    InProgress = 0,
    Completed = 1,
}
