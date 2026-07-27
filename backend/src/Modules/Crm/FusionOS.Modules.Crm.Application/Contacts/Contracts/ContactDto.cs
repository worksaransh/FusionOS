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
    DateTimeOffset CreatedAt,
    bool IsPossibleDuplicate = false);

/// <summary>Single place that turns a Contact aggregate into its DTO, shared by every handler that returns one.</summary>
public static class ContactMapper
{
    /// <summary>Plain mapping with no duplicate-warning context — used by every read path (List/GetById) where the concept doesn't apply.</summary>
    public static ContactDto ToDto(Domain.Contacts.Contact contact) => ToDto(contact, isPossibleDuplicate: false);

    /// <summary>Used by CreateContactCommandHandler to attach the advisory IsPossibleDuplicate flag it computed before creating.</summary>
    public static ContactDto ToDto(Domain.Contacts.Contact contact, bool isPossibleDuplicate) => new(
        contact.Id,
        contact.Name,
        contact.Email,
        contact.Phone,
        contact.Title,
        contact.AccountId,
        contact.LeadId,
        contact.IsActive,
        contact.CreatedAt,
        isPossibleDuplicate);
}
