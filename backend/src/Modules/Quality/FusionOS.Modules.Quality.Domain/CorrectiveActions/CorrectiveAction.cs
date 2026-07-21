using FusionOS.SharedKernel;
using FusionOS.Modules.Quality.Domain.CorrectiveActions.Events;

namespace FusionOS.Modules.Quality.Domain.CorrectiveActions;

/// <summary>
/// Phase 5 — Quality: a Corrective and Preventive Action (CAPA) plan raised against a
/// <see cref="NonConformanceReports.NonConformanceReport"/> — root cause, the corrective fix,
/// and the preventive measure to stop recurrence, assigned to a user with a due date.
/// NonConformanceReportId is a same-module reference (the NCR lives in this same Quality
/// module), existence-validated by the command handler — same convention as
/// MaintenanceRequest validating AssetId via IAssetRepository (Maintenance module).
/// AssignedToUserId is an opaque cross-module reference into Core's User — never
/// existence-validated here, same convention as Warehouse's PickList.AssignedToUserId.
///
/// Lifecycle: Open -> InProgress -> Closed -> Verified, forward-only, same discipline as
/// MaintenanceRequest's Open -> InProgress -> Completed. Verified is a distinct final step
/// from Closed: Close() records the work as done, Verify() is a separate reviewer
/// confirming the fix was actually effective — the minimal review/sign-off gate this
/// aggregate needs.
///
/// Core's generic ApprovalRequest/ApprovalStep workflow engine (Core.Domain.Workflow) was
/// evaluated as the backing for this Verified step and deliberately NOT used: that engine
/// requires the caller to submit an explicit ordered list of approver user ids up front via
/// a separate CreateApprovalRequestCommand, then record the decision via a separate
/// DecideApprovalStepCommand — both living in Core.Application. Routing CAPA verification
/// through it would make Quality.Application take a compile-time dependency on Core's CQRS
/// types, which no module in this codebase does today (every existing cross-module
/// reference is an opaque id per 03_SYSTEM_ARCHITECTURE.md §2 — modules reference each
/// other's data, never each other's command/query types), and there is no compiler
/// available in this environment to verify a first-of-its-kind wiring like that actually
/// resolves. A single in-aggregate Verify() step — same shape as MaintenanceRequest's
/// Start/Complete — is the safer minimal fit for this slice. Routing CAPA verification
/// through the generic engine (e.g. once a real multi-approver CAPA sign-off policy is
/// needed) is a reasonable, separately-scoped follow-up.
/// </summary>
public sealed class CorrectiveAction : TenantAggregateRoot
{
    public Guid NonConformanceReportId { get; private set; }
    public string RootCauseDescription { get; private set; } = default!;
    public string CorrectiveActionDescription { get; private set; } = default!;
    public string PreventiveActionDescription { get; private set; } = default!;
    public Guid AssignedToUserId { get; private set; }
    public DateTimeOffset DueDate { get; private set; }
    public CorrectiveActionStatus Status { get; private set; }
    public DateTimeOffset? ClosedAt { get; private set; }
    public DateTimeOffset? VerifiedAt { get; private set; }

    private CorrectiveAction() { }

    public static CorrectiveAction Create(
        Guid companyId,
        Guid nonConformanceReportId,
        string rootCauseDescription,
        string correctiveActionDescription,
        string preventiveActionDescription,
        Guid assignedToUserId,
        DateTimeOffset dueDate)
    {
        if (nonConformanceReportId == Guid.Empty)
            throw new ArgumentException("Non-conformance report id is required.", nameof(nonConformanceReportId));
        if (string.IsNullOrWhiteSpace(rootCauseDescription))
            throw new ArgumentException("Root cause description is required.", nameof(rootCauseDescription));
        if (string.IsNullOrWhiteSpace(correctiveActionDescription))
            throw new ArgumentException("Corrective action description is required.", nameof(correctiveActionDescription));
        if (string.IsNullOrWhiteSpace(preventiveActionDescription))
            throw new ArgumentException("Preventive action description is required.", nameof(preventiveActionDescription));
        if (assignedToUserId == Guid.Empty)
            throw new ArgumentException("Assigned-to user id is required.", nameof(assignedToUserId));
        if (dueDate == default)
            throw new ArgumentException("Due date is required.", nameof(dueDate));

        var capa = new CorrectiveAction
        {
            CompanyId = companyId,
            NonConformanceReportId = nonConformanceReportId,
            RootCauseDescription = rootCauseDescription.Trim(),
            CorrectiveActionDescription = correctiveActionDescription.Trim(),
            PreventiveActionDescription = preventiveActionDescription.Trim(),
            AssignedToUserId = assignedToUserId,
            DueDate = dueDate,
            Status = CorrectiveActionStatus.Open,
        };

        capa.Raise(new CorrectiveActionCreated(capa.Id, companyId, nonConformanceReportId, assignedToUserId));
        return capa;
    }

    /// <summary>Marks work as underway. Requires the plan to still be Open.</summary>
    public void Start()
    {
        if (Status != CorrectiveActionStatus.Open)
            throw new InvalidOperationException($"Only an Open corrective action can be started (current status: {Status}).");

        Status = CorrectiveActionStatus.InProgress;
    }

    /// <summary>Records the corrective/preventive work as done. Requires it to be InProgress.</summary>
    public void Close()
    {
        if (Status != CorrectiveActionStatus.InProgress)
            throw new InvalidOperationException($"Only an InProgress corrective action can be closed (current status: {Status}).");

        Status = CorrectiveActionStatus.Closed;
        ClosedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>A reviewer confirms the closed fix was actually effective. Requires it to already be Closed.</summary>
    public void Verify()
    {
        if (Status != CorrectiveActionStatus.Closed)
            throw new InvalidOperationException($"Only a Closed corrective action can be verified (current status: {Status}).");

        Status = CorrectiveActionStatus.Verified;
        VerifiedAt = DateTimeOffset.UtcNow;
        Raise(new CorrectiveActionVerified(Id, CompanyId, NonConformanceReportId, AssignedToUserId));
    }
}
