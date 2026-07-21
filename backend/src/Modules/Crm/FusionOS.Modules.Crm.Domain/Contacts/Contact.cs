using FusionOS.SharedKernel;
using FusionOS.Modules.Crm.Domain.Contacts.Events;

namespace FusionOS.Modules.Crm.Domain.Contacts;

/// <summary>
/// Phase 4 — CRM. A named individual with contact details — a person, not an
/// organization (see Account for the org/company side). A Contact usually
/// belongs to an Account, but one can exist before an Account does: it may be
/// captured straight off a Lead (same idea as Lead itself preceding the Sales
/// Customer it eventually produces). Both <see cref="AccountId"/> and
/// <see cref="LeadId"/> are opaque same-module references — validated by the
/// command handler that sets them (existence checked via the target's own
/// repository), never enforced here, same division of responsibility as
/// Customer.AssignPriceList.
/// </summary>
public sealed class Contact : TenantAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public string? Title { get; private set; }
    public Guid? AccountId { get; private set; }
    public Guid? LeadId { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Contact() { }

    public static Contact Create(Guid companyId, string name, string? email, string? phone, string? title, Guid? accountId, Guid? leadId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Contact name is required.", nameof(name));
        if (email is not null && !email.Contains('@'))
            throw new ArgumentException("Email must be a valid email address.", nameof(email));

        var contact = new Contact
        {
            CompanyId = companyId,
            Name = name.Trim(),
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant(),
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
            AccountId = accountId,
            LeadId = leadId,
        };

        contact.Raise(new ContactCreated(contact.Id, companyId, contact.Name));
        return contact;
    }

    /// <summary>
    /// Covers every mutable field, including the Account/Lead links — this is how a
    /// contact captured against a Lead gets re-pointed at the real Account once one
    /// exists (mirrors Customer.UpdateDetails' "everything but the immutable key" shape).
    /// </summary>
    public void UpdateDetails(string name, string? email, string? phone, string? title, Guid? accountId, Guid? leadId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Contact name is required.", nameof(name));
        if (email is not null && !email.Contains('@'))
            throw new ArgumentException("Email must be a valid email address.", nameof(email));

        Name = name.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
        Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim();
        AccountId = accountId;
        LeadId = leadId;
    }

    /// <summary>Soft-deactivate only — never a hard delete (same convention as Customer.Deactivate).</summary>
    public void Deactivate() => IsActive = false;
}
