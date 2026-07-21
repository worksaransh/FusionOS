using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Contacts.Contracts;

namespace FusionOS.Modules.Crm.Application.Contacts.Queries.GetContactById;

public sealed record GetContactByIdQuery(Guid CompanyId, Guid ContactId) : IQuery<ContactDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "crm.contact.read" };
}
