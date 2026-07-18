using FusionOS.SharedKernel;
using FusionOS.Modules.Core.Domain.Workflow.Events;

namespace FusionOS.Modules.Core.Domain.Workflow;

/// <summary>
/// Generic, module-agnostic multi-step approval chain (Phase M7, 2026-07-15)
/// — the "Workflow/Approval engine" named in docs/REMEDIATION_ROADMAP.md,
/// replacing the idea of every module inventing its own one-off Approve()
/// (e.g. Procurement's PurchaseOrder.Approve()). EntityType/EntityId are
/// opaque references into whatever aggregate this approval is for — same
/// no-cross-module-FK convention as everywhere else (03_SYSTEM_ARCHITECTURE.md
/// §2); this engine doesn't know or care what a "PurchaseOrder" is, it only
/// tracks the approval workflow around some (EntityType, EntityId) pair.
///
/// Deliberately NOT wired into PurchaseOrder.Approve() in this phase — that
/// handler is tested, working code, and refactoring it to route through this
/// new engine is a separate, later migration, not bundled in here where no
/// compiler exists in this environment to verify the refactor didn't break
/// anything. This phase's job is to make the generic engine exist and be
/// independently correct/usable via its own API, not to retrofit every
/// existing approval flow in one pass.
///
/// Approvers are supplied by the caller at submission time (an ordered list
/// of specific user ids) — this engine has no opinion on "who should approve
/// a PO over $10,000" or any other business policy; that decision belongs to
/// whoever calls CreateApprovalRequestCommand, same as this codebase's
/// existing rule to not invent business policy it hasn't been asked for
/// (see the tax-jurisdiction/costing-method/notification-provider decisions
/// still pending in docs/PROJECT_TRACKER.md §4).
/// </summary>
public sealed class ApprovalRequest : TenantAggregateRoot
{
    private readonly List<ApprovalStep> _steps = new();

    public string EntityType { get; private set; } = default!;
    public Guid EntityId { get; private set; }
    public Guid RequestedBy { get; private set; }
    public ApprovalStatus Status { get; private set; }
    public int CurrentStepNumber { get; private set; }
    public IReadOnlyList<ApprovalStep> Steps => _steps.AsReadOnly();

    private ApprovalRequest() { }

    public static ApprovalRequest Submit(Guid companyId, string entityType, Guid entityId, Guid requestedBy, IReadOnlyList<Guid> approverUserIds)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required.", nameof(entityType));
        if (entityId == Guid.Empty)
            throw new ArgumentException("Entity id is required.", nameof(entityId));
        if (requestedBy == Guid.Empty)
            throw new ArgumentException("Requested-by user id is required.", nameof(requestedBy));
        if (approverUserIds is null || approverUserIds.Count == 0)
            throw new ArgumentException("At least one approval step is required.", nameof(approverUserIds));
        if (approverUserIds.Any(id => id == requestedBy))
            throw new ArgumentException("The requester cannot also be one of the approvers (maker-checker).", nameof(approverUserIds));

        var request = new ApprovalRequest
        {
            CompanyId = companyId,
            EntityType = entityType.Trim(),
            EntityId = entityId,
            RequestedBy = requestedBy,
            Status = ApprovalStatus.Pending,
            CurrentStepNumber = 1,
        };

        var stepNumber = 1;
        foreach (var approverUserId in approverUserIds)
        {
            request._steps.Add(ApprovalStep.Create(stepNumber, approverUserId));
            stepNumber++;
        }

        request.Raise(new ApprovalRequestSubmitted(request.Id, companyId, request.EntityType, entityId, requestedBy, approverUserIds[0]));
        return request;
    }

    /// <summary>
    /// Records a decision on whatever step is currently pending. Only that
    /// step's assigned approver may decide it (enforced here, not just via
    /// permission — same "data-dependent authorization the MediatR pipeline
    /// can't express" pattern already used for PO's maker-checker check).
    /// Rejecting any step halts the whole chain immediately; approving the
    /// last step completes it.
    /// </summary>
    public void Decide(Guid actingUserId, bool approve, string? comments)
    {
        if (Status != ApprovalStatus.Pending)
            throw new InvalidOperationException($"This approval request is already {Status} — no further decisions can be recorded.");

        var currentStep = _steps.SingleOrDefault(s => s.StepNumber == CurrentStepNumber)
            ?? throw new InvalidOperationException("No current pending step found for this approval request.");

        if (currentStep.ApproverUserId != actingUserId)
            throw new InvalidOperationException("Only this step's assigned approver can record a decision on it.");

        if (approve)
        {
            currentStep.Approve(comments);

            if (CurrentStepNumber == _steps.Count)
            {
                Status = ApprovalStatus.Approved;
                Raise(new ApprovalRequestApproved(Id, CompanyId, EntityType, EntityId, RequestedBy));
            }
            else
            {
                CurrentStepNumber++;
                var nextApproverUserId = _steps.Single(s => s.StepNumber == CurrentStepNumber).ApproverUserId;
                Raise(new ApprovalStepAdvanced(Id, CompanyId, EntityType, EntityId, CurrentStepNumber, nextApproverUserId));
            }
        }
        else
        {
            currentStep.Reject(comments);
            Status = ApprovalStatus.Rejected;
            Raise(new ApprovalRequestRejected(Id, CompanyId, EntityType, EntityId, RequestedBy, currentStep.StepNumber));
        }
    }
}
