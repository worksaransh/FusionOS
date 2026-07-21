using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Contacts.Contracts;

namespace FusionOS.Modules.Crm.Application.Contacts.Commands.DeactivateContact;

/// <summary>Soft-deactivate only — see Contact.Deactivate(). Never a hard delete.</summary>
public sealed record DeactivateContactCommand(Guid CompanyId, Guid ContactId)
    : ICommand<ContactDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.contact.deactivate" };
    public string EntityType => nameof(Domain.Contacts.Contact);
    public Guid EntityId => ContactId;
    public string Action => "Deactivated";
}
