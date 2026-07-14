using FusionOS.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.BuildingBlocks.Infrastructure.Persistence;

public class Repository<TAggregate> : IRepository<TAggregate> where TAggregate : AggregateRoot
{
    private readonly DbContext _context;
    private readonly DbSet<TAggregate> _set;

    public Repository(DbContext context)
    {
        _context = context;
        _set = context.Set<TAggregate>();
    }

    public async Task<TAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _set.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default) =>
        await _set.AddAsync(aggregate, cancellationToken);

    public void Update(TAggregate aggregate) => _context.Update(aggregate);

    /// <summary>Soft-delete only — 04_DATABASE_GUIDELINES.md §4 forbids hard deletes.
    /// Callers must mark the aggregate deleted via its own domain method before
    /// calling this; this simply ensures EF tracks the state as Modified.</summary>
    public void Remove(TAggregate aggregate) => _context.Update(aggregate);
}
