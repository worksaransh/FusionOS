namespace FusionOS.Modules.Sales.Application.Dispatches.Contracts;

public interface IDispatchRepository
{
    Task AddAsync(Domain.Dispatches.Dispatch dispatch, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Dispatches.Dispatch>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sums the quantity already dispatched for one product across every existing
    /// dispatch against this sales order (2026-07-14 coverage-audit follow-up:
    /// CreateDispatchCommandHandler previously fetched the sales order only to
    /// check it existed, never to bound how much of it could be dispatched).
    /// Counting rule (2026-07-20): every persisted dispatch counts - Dispatch
    /// has no status/lifecycle (no cancelled or returned state), so each line is
    /// goods that physically left against this order. If a cancellation/return
    /// state is ever added, this sum must exclude it.
    /// </summary>
    Task<decimal> GetDispatchedQuantityAsync(Guid companyId, Guid salesOrderId, Guid productId, CancellationToken cancellationToken = default);
}
