using FusionOS.SharedKernel;

namespace FusionOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>Minimal repository abstraction per 01_PROJECT_RULES.md — used for aggregate persistence, not a blanket wrapper over every table.</summary>
public interface IRepository<TAggregate> where TAggregate : AggregateRoot
{
    Task<TAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
    void Update(TAggregate aggregate);
    void Remove(TAggregate aggregate);
}
