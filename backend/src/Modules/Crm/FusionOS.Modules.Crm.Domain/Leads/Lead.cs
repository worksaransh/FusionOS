using FusionOS.SharedKernel;
using FusionOS.Modules.Crm.Domain.Leads.Events;

namespace FusionOS.Modules.Crm.Domain.Leads;

/// <summary>
/// Phase 4 — CRM, first slice. A raw prospect captured before they become a Sales
/// Customer: contact details, a free-text source, and a New → Qualified →
/// Converted / Disqualified lifecycle. A Lead never creates a Sales Customer itself —
/// that happens downstream when an <c>Opportunity</c> raised from a qualified lead is
/// won (see Opportunity). Converting a lead here only records that an opportunity was
/// opened from it; it is a pure CRM-side state change.
/// </summary>
public sealed class Lead : TenantAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public string? Source { get; private set; }
    public LeadStatus Status { get; private set; }

    private Lead() { }

    public static Lead Create(Guid companyId, string name, string? contactEmail, string? contactPhone, string? source)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Lead name is required.", nameof(name));
        if (contactEmail is not null && !contactEmail.Contains('@'))
            throw new ArgumentException("Contact email must be a valid email address.", nameof(contactEmail));

        var lead = new Lead
        {
            CompanyId = companyId,
            Name = name.Trim(),
            ContactEmail = string.IsNullOrWhiteSpace(contactEmail) ? null : contactEmail.Trim().ToLowerInvariant(),
            ContactPhone = string.IsNullOrWhiteSpace(contactPhone) ? null : contactPhone.Trim(),
            Source = string.IsNullOrWhiteSpace(source) ? null : source.Trim(),
            Status = LeadStatus.New,
        };

        lead.Raise(new LeadCreated(lead.Id, companyId, lead.Name));
        return lead;
    }

    /// <summary>New → Qualified: the lead is worth pursuing and can have an opportunity opened from it.</summary>
    public void Qualify()
    {
        if (Status != LeadStatus.New)
            throw new InvalidOperationException($"Only a New lead can be qualified (current status: {Status}).");

        Status = LeadStatus.Qualified;
    }

    /// <summary>Marks the lead a dead end. A converted lead cannot be disqualified after the fact.</summary>
    public void Disqualify()
    {
        if (Status == LeadStatus.Converted)
            throw new InvalidOperationException("A converted lead cannot be disqualified.");
        if (Status == LeadStatus.Disqualified)
            throw new InvalidOperationException("This lead is already disqualified.");

        Status = LeadStatus.Disqualified;
    }

    /// <summary>Qualified → Converted: called when an Opportunity is opened from this lead (same-module, same transaction).</summary>
    public void MarkConverted()
    {
        if (Status != LeadStatus.Qualified)
            throw new InvalidOperationException($"Only a Qualified lead can be converted into an opportunity (current status: {Status}).");

        Status = LeadStatus.Converted;
    }
}
