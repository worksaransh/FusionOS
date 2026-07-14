namespace FusionOS.SharedKernel.Events;

/// <summary>
/// Base for cross-module integration events published via the transactional
/// outbox (03_SYSTEM_ARCHITECTURE.md §4.2). EventType follows the
/// "Module.EntityAction.vN" convention, e.g. "Sales.SalesOrderConfirmed.v1".
/// </summary>
public abstract record IntegrationEvent(string EventType)
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
    public Guid CompanyId { get; init; }
    public string Source { get; init; } = "fusionos";
}
