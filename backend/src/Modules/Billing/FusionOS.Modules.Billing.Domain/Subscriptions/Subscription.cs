using FusionOS.SharedKernel;

namespace FusionOS.Modules.Billing.Domain.Subscriptions;

/// <summary>
/// A tenant's (CompanyId, inherited from TenantAggregateRoot — the subscribing company, not
/// duplicated as a separate field) subscription to a Plan. State machine: Trial is the only
/// entry state (Create always starts a subscription as Trial, per Phase 1.3's scope — nothing
/// here talks to a real payment gateway), Active/PastDue/Cancelled are reached only through the
/// guarded transition methods below, following the same "guard clause throws
/// InvalidOperationException, never a silent no-op" convention as
/// FusionOS.Modules.Procurement.Domain.PurchaseOrders.PurchaseOrder's Approve/Reject/Cancel.
/// </summary>
public sealed class Subscription : TenantAggregateRoot
{
    public Guid PlanId { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? TrialEndsAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }

    private Subscription() { }

    public static Subscription Create(Guid companyId, Guid planId, DateTimeOffset? trialEndsAt)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("Company id is required.", nameof(companyId));
        if (planId == Guid.Empty)
            throw new ArgumentException("Plan id is required.", nameof(planId));

        return new Subscription
        {
            CompanyId = companyId,
            PlanId = planId,
            Status = SubscriptionStatus.Trial,
            StartedAt = DateTimeOffset.UtcNow,
            TrialEndsAt = trialEndsAt,
        };
    }

    /// <summary>Converts a Trial (the normal path) or a PastDue subscription (payment caught up) into Active.</summary>
    public void Activate()
    {
        if (Status != SubscriptionStatus.Trial && Status != SubscriptionStatus.PastDue)
            throw new InvalidOperationException($"Only a Trial or PastDue subscription can be activated (current status: {Status}).");

        Status = SubscriptionStatus.Active;
    }

    /// <summary>Flags a currently-Active subscription as behind on payment. Not reachable from Trial/Cancelled.</summary>
    public void MarkPastDue()
    {
        if (Status != SubscriptionStatus.Active)
            throw new InvalidOperationException($"Only an Active subscription can be marked past due (current status: {Status}).");

        Status = SubscriptionStatus.PastDue;
    }

    /// <summary>Cancels the subscription from any non-terminal state. Terminal — no further transition is allowed afterward.</summary>
    public void Cancel()
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new InvalidOperationException("This subscription is already cancelled.");

        Status = SubscriptionStatus.Cancelled;
        CancelledAt = DateTimeOffset.UtcNow;
    }
}
