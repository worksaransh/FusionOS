using FusionOS.SharedKernel;
using FusionOS.Modules.Crm.Domain.Activities.Events;

namespace FusionOS.Modules.Crm.Domain.Activities;

/// <summary>
/// Phase 4 — CRM. A logged interaction (call/email/meeting/note) against some other
/// CRM record — a Lead, Opportunity, Account, or Contact. Uses the same opaque
/// (EntityType, EntityId) polymorphic-reference convention as Core's
/// <c>ApprovalRequest</c> rather than four nullable FK columns: this aggregate
/// doesn't know or care what a "Lead" is, it only tracks a logged interaction
/// against some (EntityType, EntityId) pair — validated by the command handler
/// that creates it, not enforced here.
///
/// An activity is a point-in-time log entry, not a lifecycle aggregate — there is
/// deliberately no Update/Deactivate here (same reasoning a ledger posting or an
/// audit-log row is never edited after the fact). "Timestamp" and "created by
/// user" are the inherited audit fields (CreatedAt/CreatedBy) rather than
/// duplicate domain properties.
/// </summary>
public sealed class Activity : TenantAggregateRoot
{
    public string EntityType { get; private set; } = default!;
    public Guid EntityId { get; private set; }
    public ActivityType Type { get; private set; }
    public string Notes { get; private set; } = default!;

    private Activity() { }

    public static Activity Log(Guid companyId, string entityType, Guid entityId, ActivityType type, string notes)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required.", nameof(entityType));
        if (entityId == Guid.Empty)
            throw new ArgumentException("Entity id is required.", nameof(entityId));
        if (string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException("Notes are required.", nameof(notes));

        var activity = new Activity
        {
            CompanyId = companyId,
            EntityType = entityType.Trim(),
            EntityId = entityId,
            Type = type,
            Notes = notes.Trim(),
        };

        activity.Raise(new ActivityLogged(activity.Id, companyId, activity.EntityType, entityId, type));
        return activity;
    }
}
