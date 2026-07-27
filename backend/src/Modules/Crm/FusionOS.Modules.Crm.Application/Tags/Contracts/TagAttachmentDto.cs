namespace FusionOS.Modules.Crm.Application.Tags.Contracts;

/// <summary>The join-row shape (EntityTag), returned by Attach/Detach. Read paths for "what tags does this entity carry" go through ListTagsForEntityQuery and return TagDto instead, joined with tag Name/Color for direct UI badge rendering.</summary>
public sealed record TagAttachmentDto(Guid Id, Guid TagId, string EntityType, Guid EntityId);

/// <summary>Single place that turns an EntityTag aggregate into its DTO, shared by every handler that returns one.</summary>
public static class EntityTagMapper
{
    public static TagAttachmentDto ToDto(Domain.Tags.EntityTag entityTag) =>
        new(entityTag.Id, entityTag.TagId, entityTag.EntityType, entityTag.EntityId);
}
