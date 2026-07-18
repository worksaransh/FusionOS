namespace FusionOS.Modules.Warehouse.Application.Bins.Contracts;

public interface IBinRepository
{
    Task<Domain.Bins.Bin?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ZoneExistsAsync(Guid companyId, Guid zoneId, CancellationToken cancellationToken = default);
    /// <summary>Company-scoped existence check (not zone-scoped) — used by PickList's Create handler, which only knows a BinId, not necessarily its ZoneId ahead of time.</summary>
    Task<bool> ExistsAsync(Guid companyId, Guid binId, CancellationToken cancellationToken = default);
    /// <summary>
    /// The first active Bin in a Zone, ordered by Code — the Putaway slice's entire
    /// "suggestion" heuristic (docs/IMPLEMENTATION_PLAN.md item 12). Deliberately not
    /// a real slotting algorithm (nearest-empty-bin, last-bin-used-for-this-product,
    /// capacity-aware, etc.) — building one is a distinct, much larger piece of work
    /// with no spec behind it yet; this is a documented placeholder a worker can
    /// always override via ConfirmPutaway, same restraint as the Dashboard's
    /// hardcoded 10-unit low-stock threshold in Phase M6.
    /// </summary>
    Task<Domain.Bins.Bin?> GetFirstActiveBinAsync(Guid companyId, Guid zoneId, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(Guid companyId, Guid zoneId, string code, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Bins.Bin bin, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Bins.Bin>> ListAsync(Guid companyId, Guid zoneId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid zoneId, CancellationToken cancellationToken = default);
}
