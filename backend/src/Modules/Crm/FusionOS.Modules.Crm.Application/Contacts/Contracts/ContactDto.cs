namespace FusionOS.Modules.Crm.Application.Contacts.Contracts;

public sealed record ContactDto(
    Guid Id,
    string Name,
    string? Email,
    string? Phone,
    string? Title,
    Guid? AccountId,
    Guid? LeadId,
    bool IsActive,
    DateTimeOffset CreatedAt);

/// <summary>Single place that turns a Contact aggregate into its DTO, shared by every handler that returns one.</summary>
public static class ContactMapper
{
    public static ContactDto ToDto(Domain.Contacts.Contact contact) => new(
        contact.Id,
        contact.Name,
        contact.Email,
        contact.Phone,
        contact.Title,
        contact.AccountId,
        contact.LeadId,
        contact.IsActive,
        contact.CreatedAt);
}
