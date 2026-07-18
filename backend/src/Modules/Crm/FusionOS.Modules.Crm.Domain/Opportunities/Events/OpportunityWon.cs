using FusionOS.SharedKernel;

namespace FusionOS.Modules.Crm.Domain.Opportunities.Events;

/// <summary>
/// Raised when an opportunity is won — the point CRM hands off to Sales. Relayed via the
/// outbox to Kafka (03_SYSTEM_ARCHITECTURE.md §4.2) and consumed by Sales'
/// OpportunityWonConsumer, which creates the real Customer via Customer.Create using the
/// snapshotted prospect details and the code chosen at win time. CRM never creates a Sales
/// Customer directly — it announces the win and lets Sales, which owns Customer, apply it
/// (same producer/consumer split as WorkOrderCompleted → Inventory).
/// </summary>
public sealed record OpportunityWon(
    Guid OpportunityId,
    Guid CompanyId,
    string CustomerName,
    string CustomerCode,
    string? ContactEmail) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
