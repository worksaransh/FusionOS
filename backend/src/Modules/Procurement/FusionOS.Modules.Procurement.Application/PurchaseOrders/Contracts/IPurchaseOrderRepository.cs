namespace FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;

public interface IPurchaseOrderRepository
{
    Task<Domain.PurchaseOrders.PurchaseOrder?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.PurchaseOrders.PurchaseOrder order, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.PurchaseOrders.PurchaseOrder>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>Count of purchase orders per status, for the PO status summary report (Phase M6, 2026-07-15). Statuses with zero orders are simply absent — the handler fills them in as zero.</summary>
    Task<IReadOnlyList<(Domain.PurchaseOrders.PurchaseOrderStatus Status, int Count)>> CountByStatusAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>Per-supplier order count/value/fulfillment stats for the supplier scorecard report (Phase 10 item 2). Only suppliers with at least one purchase order appear — a supplier with zero orders has nothing to score yet.</summary>
    Task<IReadOnlyList<(Guid SupplierId, int OrderCount, decimal TotalOrderValue, int FullyReceivedCount)>> GetSupplierOrderStatsAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Every historical unit price paid for a Product, one row per PurchaseOrderLine
    /// across every PurchaseOrder that ordered it — 05_MODULE_ROADMAP.md's Procurement
    /// "Price history" line item (Phase 1 closeout, 2026-07-18). Derived entirely from
    /// existing PurchaseOrder data rather than a new aggregate that would just duplicate
    /// it — same "canned report over existing data" shape as the PO status summary and
    /// supplier scorecard reports above, not a new domain concept. Ordered oldest-first
    /// so callers can plot a trend.
    /// </summary>
    Task<IReadOnlyList<(Guid PurchaseOrderId, Guid SupplierId, DateTimeOffset OrderDate, decimal UnitPrice, decimal Quantity)>> GetPriceHistoryAsync(Guid companyId, Guid productId, CancellationToken cancellationToken = default);
}
