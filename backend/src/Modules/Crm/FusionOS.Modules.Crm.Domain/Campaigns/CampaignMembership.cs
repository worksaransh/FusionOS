using FusionOS.SharedKernel;

namespace FusionOS.Modules.Crm.Domain.Campaigns;

/// <summary>
/// Phase 4 — CRM, marketing campaign slice. Records that one (EntityType, EntityId)
/// pair — a Lead or a Contact — belongs to a given Campaign. Uses the same opaque
/// polymorphic-reference convention as Activity's (EntityType, EntityId): this
/// aggregate doesn't know or care what a "Lead" is, it only tracks membership of
/// some (EntityType, EntityId) pair in a Campaign — validated by the command
/// handler that creates it, not enforced here. CampaignId is a real, same-module
/// foreign key (Campaign lives in this module), validated by the handler via
/// ICampaignRepository, same convention as CreateBudgetLine validating AccountId
/// in Finance.
///
/// A membership row is a point-in-time fact, not a lifecycle aggregate — there is
/// deliberately no Update here, only Create and a hard Remove (handled at the
/// repository level, same as removing a row from a join table) once the member is
/// taken off the campaign — unlike Activity, a membership fact really can become
/// wrong (someone enrolled by mistake) rather than merely superseded.
/// </summary>
public sealed class CampaignMembership : TenantAggregateRoot
{
    public Guid CampaignId { get; private set; }
    public string EntityType { get; private set; } = default!;
    public Guid EntityId { get; private set; }
    public DateTimeOffset AddedAt { get; private set; }

    private CampaignMembership() { }

    public static CampaignMembership Create(Guid companyId, Guid campaignId, string entityType, Guid entityId)
    {
        if (campaignId == Guid.Empty)
            throw new ArgumentException("Campaign id is required.", nameof(campaignId));
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required.", nameof(entityType));
        if (entityId == Guid.Empty)
            throw new ArgumentException("Entity id is required.", nameof(entityId));

        return new CampaignMembership
        {
            CompanyId = companyId,
            CampaignId = campaignId,
            EntityType = entityType.Trim(),
            EntityId = entityId,
            AddedAt = DateTimeOffset.UtcNow,
        };
    }
}
