using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Tags.Contracts;

namespace FusionOS.Modules.Crm.Application.Tags.Queries.ListTags;

/// <summary>
/// Reuses crm.tag.manage for reads rather than a dedicated crm.tag.read — no such
/// permission is reserved in PermissionCatalog (only crm.tag.manage is), and tags are
/// low-sensitivity master data (labels, not customer data) shared by whoever can
/// manage them, same reasoning CRM's own AssignLeadAccount reuses crm.lead.assign-account
/// rather than splitting read/write further than the catalog already does.
/// </summary>
public sealed record ListTagsQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<TagDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "crm.tag.manage" };
}
