using FusionOS.SharedKernel;
using FusionOS.Modules.Hrms.Domain.Onboarding.Events;

namespace FusionOS.Modules.Hrms.Domain.Onboarding;

/// <summary>
/// Onboarding and Offboarding depth pass (2026-07-23): a checklist-style workflow attached to an
/// Employee — 05_MODULE_ROADMAP.md's Onboarding/Offboarding line items.
///
/// <b>Design decision — one entity, not two.</b> Onboarding and Offboarding are structurally
/// identical (an employee, a caller-supplied checklist of tasks, InProgress -> Completed) and the
/// only real difference between them is which permission code guards each action and which label
/// a user sees. Two near-duplicate aggregate classes (OnboardingWorkflow/OffboardingWorkflow)
/// would mean two copies of every task-completion/auto-complete rule to keep in sync for zero
/// behavioral gain, so this is a single <see cref="EmployeeWorkflow"/> parameterized by
/// <see cref="Kind"/> instead. What stays deliberately separate is the Application layer: distinct
/// command/query types per direction (Commands/CreateOnboardingWorkflow vs
/// Commands/CreateOffboardingWorkflow, etc.) so each carries its own RequiredPermissions
/// (hrms.onboarding.* vs hrms.offboarding.*) — the permission catalog already reserves both sets
/// separately, and IRequirePermission is per-command, so this is also the only way to keep
/// onboarding and offboarding independently permissioned while sharing the underlying entity.
/// This class physically lives under Domain/Onboarding/ rather than duplicated (or split) across
/// Domain/Onboarding/ and Domain/Offboarding/ for the same reason — one entity, one home.
///
/// <b>EmployeeId</b> is a same-module reference (Employee lives in this module), validated by the
/// command handler via IEmployeeRepository — same convention as LeaveRequest.EmployeeId/
/// AttendanceRecord.EmployeeId/PayrollRecord.EmployeeId.
///
/// <b>Auto-completion.</b> There is no separate explicit "close this workflow" action — see
/// <see cref="CompleteTask"/> for exactly when/why completing a task can also complete the whole
/// workflow. This is a deliberate simplification: a checklist whose last box was just ticked is,
/// by definition, done. A future slice could add an explicit "close with incomplete tasks" escape
/// hatch if a real user needs one; nothing in this pass calls for it.
///
/// <b>Offboarding and Employee.Deactivate() are deliberately NOT coupled.</b> Starting (or
/// completing) an offboarding workflow does not itself deactivate the employee — that remains a
/// separate, explicit action via DeactivateEmployeeCommand, performed independently. Deactivation
/// is a serious, standalone business action and should never be a side effect of a workflow's
/// internal task bookkeeping.
///
/// A "template library" of standard onboarding/offboarding checklists (so callers don't have to
/// retype the same task list every time) is a reasonable future follow-up — out of scope here;
/// <see cref="Create"/> always takes the checklist from the caller, never a fixed/hardcoded one.
/// </summary>
public sealed class EmployeeWorkflow : TenantAggregateRoot
{
    private readonly List<WorkflowTask> _tasks = new();

    public Guid EmployeeId { get; private set; }
    public WorkflowKind Kind { get; private set; }
    public WorkflowStatus Status { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }
    public IReadOnlyList<WorkflowTask> Tasks => _tasks.AsReadOnly();

    private EmployeeWorkflow() { }

    public static EmployeeWorkflow Create(Guid companyId, Guid employeeId, WorkflowKind kind, IReadOnlyCollection<string> taskTitles, DateTimeOffset startedAt)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee id is required.", nameof(employeeId));
        if (taskTitles is null || taskTitles.Count == 0)
            throw new ArgumentException("A workflow must have at least one checklist task.", nameof(taskTitles));

        var workflow = new EmployeeWorkflow
        {
            CompanyId = companyId,
            EmployeeId = employeeId,
            Kind = kind,
            Status = WorkflowStatus.InProgress,
            StartedAt = startedAt,
        };

        foreach (var title in taskTitles)
            workflow._tasks.Add(WorkflowTask.Create(title));

        workflow.Raise(new EmployeeWorkflowStarted(workflow.Id, companyId, employeeId, kind.ToString()));
        return workflow;
    }

    /// <summary>
    /// Marks one checklist task done. If every task on this workflow is now completed, the whole
    /// workflow auto-transitions InProgress -> Completed and stamps <see cref="CompletedAt"/> in
    /// the same call — there is no separate "close the workflow" step (see class doc comment).
    /// </summary>
    public void CompleteTask(Guid taskId, DateTimeOffset completedAt, string? notes = null)
    {
        if (Status == WorkflowStatus.Completed)
            throw new InvalidOperationException("This workflow is already completed.");

        var task = _tasks.FirstOrDefault(t => t.Id == taskId)
            ?? throw new ArgumentException($"Task '{taskId}' is not part of this workflow.", nameof(taskId));

        task.Complete(completedAt, notes);

        if (_tasks.All(t => t.IsCompleted))
        {
            Status = WorkflowStatus.Completed;
            CompletedAt = completedAt;
            Raise(new EmployeeWorkflowCompleted(Id, CompanyId, EmployeeId, Kind.ToString()));
        }
    }
}
