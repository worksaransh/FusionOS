namespace FusionOS.SharedKernel;

/// <summary>
/// Base type for aggregate roots. Holds uncommitted domain events until the
/// application layer dispatches them (via a SaveChanges interceptor) and clears them.
/// </summary>
public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();
}
