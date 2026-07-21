using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Contacts.Contracts;

namespace FusionOS.Modules.Crm.Application.Contacts.Queries.ListContacts;

public sealed record ListContactsQuery(Guid CompanyId, string? Search = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<ContactDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "crm.contact.read" };
}
