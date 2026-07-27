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

    /// <summary>
    /// Batch counterpart of GetDispatchedQuantityAsync (Phase 10 audit fix,
    /// 2026-07-27): CreateDispatchCommandHandler previously called
    /// GetDispatchedQuantityAsync once per distinct product in the request
    /// (an N+1 round trip per GroupBy iteration). Resolves every distinct
    /// product's already-dispatched quantity for this sales order in one
    /// query - same "batch-resolve by id set" convention as
    /// Crm's ITagRepository.GetByIdsAsync / Finance's ITaxRateRepository.GetByIdsAsync.
    /// Products with no dispatched quantity yet are simply absent from the
    /// result - callers should default to 0.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, decimal>> GetDispatchedQuantitiesAsync(Guid companyId, Guid salesOrderId, IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Customer Portal support (2026-07-22): Dispatch has no CustomerId of its
    /// own (only SalesOrderId - see Dispatch's own doc comment), so this is
    /// scoped by joining through SalesOrder.CustomerId in the Infrastructure
    /// implementation rather than filtering Dispatch directly. Same
    /// CustomerId-scoped shape/rationale as IQuotationRepository.ListByCustomerAsync.
    /// </summary>
    Task<IReadOnlyList<Domain.Dispatches.Dispatch>> ListByCustomerAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>Customer Portal support (2026-07-22) - see ListByCustomerAsync.</summary>
    Task<int> CountByCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
}
