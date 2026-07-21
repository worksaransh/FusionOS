using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Contacts.Contracts;

namespace FusionOS.Modules.Crm.Application.Contacts.Commands.CreateContact;

public sealed record CreateContactCommand(Guid CompanyId, string Name, string? Email, string? Phone, string? Title, Guid? AccountId, Guid? LeadId)
    : ICommand<ContactDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.contact.create" };
    public string EntityType => nameof(Domain.Contacts.Contact);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
