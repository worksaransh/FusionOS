using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Tags.Contracts;

namespace FusionOS.Modules.Crm.Application.Tags.Queries.ListTagsForEntity;

/// <summary>Lists the Tags (name/colour, not just the join row) currently attached to a given (EntityType, EntityId) — what a badge list on a Lead/Contact/Account/Opportunity renders.</summary>
public sealed record ListTagsForEntityQuery(Guid CompanyId, string EntityType, Guid EntityId)
    : IQuery<IReadOnlyList<TagDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "crm.tag.manage" };
}
