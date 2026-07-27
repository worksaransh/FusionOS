using FusionOS.SharedKernel;

namespace FusionOS.Modules.Crm.Domain.Tags;

/// <summary>
/// Phase 4 — CRM, segmentation/tagging slice. Records that one (EntityType,
/// EntityId) pair — a Lead, Contact, Account, or Opportunity — carries a given
/// Tag. Same opaque polymorphic-reference convention as Activity and
/// CampaignMembership's (EntityType, EntityId): this aggregate doesn't know or
/// care what a "Lead" is. TagId is a real, same-module foreign key (Tag lives in
/// this module), validated by the handler via ITagRepository.
///
/// A tag attachment is a point-in-time fact with a genuine undo (Detach) — a user
/// really can remove a tag applied by mistake — same reasoning as
/// CampaignMembership.
/// </summary>
public sealed class EntityTag : TenantAggregateRoot
{
    public Guid TagId { get; private set; }
    public string EntityType { get; private set; } = default!;
    public Guid EntityId { get; private set; }

    private EntityTag() { }

    public static EntityTag Create(Guid companyId, Guid tagId, string entityType, Guid entityId)
    {
        if (tagId == Guid.Empty)
            throw new ArgumentException("Tag id is required.", nameof(tagId));
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required.", nameof(entityType));
        if (entityId == Guid.Empty)
            throw new ArgumentException("Entity id is required.", nameof(entityId));

        return new EntityTag
        {
            CompanyId = companyId,
            TagId = tagId,
            EntityType = entityType.Trim(),
            EntityId = entityId,
        };
    }
}
