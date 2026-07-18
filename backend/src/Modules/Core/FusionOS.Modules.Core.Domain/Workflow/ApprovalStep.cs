namespace FusionOS.Modules.Core.Domain.Workflow;

/// <summary>
/// One sequential step of an ApprovalRequest's approval chain — plain class,
/// not a TenantAggregateRoot, mirroring Procurement's PurchaseOrderLine: it's
/// never queried or mutated independently of its parent ApprovalRequest, so
/// it carries no CompanyId/audit/RowVersion columns of its own. Construction
/// and mutation are `internal` so only ApprovalRequest (same assembly) can
/// touch a step — never directly from Application/Infrastructure.
/// </summary>
public sealed class ApprovalStep
{
    public Guid Id { get; private set; }
    public int StepNumber { get; private set; }
    public Guid ApproverUserId { get; private set; }
    public ApprovalStatus Decision { get; private set; } = ApprovalStatus.Pending;
    public DateTimeOffset? DecidedAt { get; private set; }
    public string? Comments { get; private set; }

    private ApprovalStep() { }

    internal static ApprovalStep Create(int stepNumber, Guid approverUserId)
    {
        if (stepNumber < 1)
            throw new ArgumentException("Step number must be 1 or greater.", nameof(stepNumber));
        if (approverUserId == Guid.Empty)
            throw new ArgumentException("Approver user id is required.", nameof(approverUserId));

        return new ApprovalStep
        {
            Id = Guid.NewGuid(),
            StepNumber = stepNumber,
            ApproverUserId = approverUserId,
        };
    }

    internal void Approve(string? comments)
    {
        Decision = ApprovalStatus.Approved;
        DecidedAt = DateTimeOffset.UtcNow;
        Comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
    }

    internal void Reject(string? comments)
    {
        Decision = ApprovalStatus.Rejected;
        DecidedAt = DateTimeOffset.UtcNow;
        Comments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
    }
}
