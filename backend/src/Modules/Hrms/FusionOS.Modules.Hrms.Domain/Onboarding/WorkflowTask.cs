namespace FusionOS.Modules.Hrms.Domain.Onboarding;

/// <summary>
/// One checklist item on an <see cref="EmployeeWorkflow"/> — a plain child entity, not an
/// aggregate root of its own (same "line entity, no audit/tenant columns" shape as
/// WorkOrderComponent). Titles are supplied by the caller when the workflow is created; there is
/// no fixed/hardcoded template (see EmployeeWorkflow.Create's own doc comment).
/// </summary>
public sealed class WorkflowTask
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = default!;
    public bool IsCompleted { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public string? Notes { get; private set; }

    private WorkflowTask() { }

    internal static WorkflowTask Create(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title is required.", nameof(title));

        return new WorkflowTask
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
            IsCompleted = false,
        };
    }

    internal void Complete(DateTimeOffset completedAt, string? notes)
    {
        if (IsCompleted)
            throw new InvalidOperationException($"Task '{Title}' is already completed.");

        IsCompleted = true;
        CompletedAt = completedAt;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}
