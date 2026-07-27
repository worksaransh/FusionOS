using FusionOS.SharedKernel;

namespace FusionOS.Modules.Crm.Domain.Campaigns.Events;

/// <summary>Raised when a marketing campaign is created. No consumer today — same "future hook" shape as LeadCreated/AccountCreated.</summary>
public sealed record CampaignCreated(Guid CampaignId, Guid CompanyId, string Name) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
