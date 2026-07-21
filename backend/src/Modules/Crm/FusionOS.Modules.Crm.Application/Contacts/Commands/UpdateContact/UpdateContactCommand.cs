using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Contacts.Contracts;

namespace FusionOS.Modules.Crm.Application.Contacts.Commands.UpdateContact;

public sealed record UpdateContactCommand(Guid CompanyId, Guid ContactId, string Name, string? Email, string? Phone, string? Title, Guid? AccountId, Guid? LeadId)
    : ICommand<ContactDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.contact.update" };
    public string EntityType => nameof(Domain.Contacts.Contact);
    public Guid EntityId => ContactId;
    public string Action => "Updated";
}
