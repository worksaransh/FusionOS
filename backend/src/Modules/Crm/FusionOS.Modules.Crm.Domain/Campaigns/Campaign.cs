using FusionOS.SharedKernel;
using FusionOS.Modules.Crm.Domain.Campaigns.Events;

namespace FusionOS.Modules.Crm.Domain.Campaigns;

/// <summary>
/// Phase 4 — CRM, marketing campaign slice. A named outreach effort (email blast,
/// trade show, webinar, etc.) that Leads and Contacts can be enrolled in. A Lead or
/// Contact can belong to many Campaigns and a Campaign can have many members, so
/// membership is deliberately NOT an owned collection here (that would force every
/// load of a Campaign to also load every member row) — it is modeled as its own
/// small aggregate, <see cref="CampaignMembership"/>, with its own table and
/// repository, same reasoning as Activity being logged separately from the entity
/// it is logged against rather than as an owned collection on that entity.
/// </summary>
public sealed class Campaign : TenantAggregateRoot
{
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset? EndDate { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Campaign() { }

    public static Campaign Create(Guid companyId, string name, string? description, DateTimeOffset startDate, DateTimeOffset? endDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Campaign name is required.", nameof(name));
        if (endDate is { } end && end < startDate)
            throw new ArgumentException("End date cannot be before the start date.", nameof(endDate));

        var campaign = new Campaign
        {
            CompanyId = companyId,
            Name = name.Trim(),
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
            StartDate = startDate,
            EndDate = endDate,
            IsActive = true,
        };

        campaign.Raise(new CampaignCreated(campaign.Id, companyId, campaign.Name));
        return campaign;
    }

    /// <summary>Soft-deactivate only — never a hard delete (same convention as Account.Deactivate).</summary>
    public void Deactivate() => IsActive = false;

    public void UpdateDetails(string name, string? description, DateTimeOffset startDate, DateTimeOffset? endDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Campaign name is required.", nameof(name));
        if (endDate is { } end && end < startDate)
            throw new ArgumentException("End date cannot be before the start date.", nameof(endDate));

        Name = name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        StartDate = startDate;
        EndDate = endDate;
    }
}
