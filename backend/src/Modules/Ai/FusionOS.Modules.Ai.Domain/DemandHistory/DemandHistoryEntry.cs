using FusionOS.SharedKernel;

namespace FusionOS.Modules.Ai.Domain.DemandHistory;

/// <summary>
/// Phase 8 — AI &amp; Integrations. A local read-model row recording one real,
/// already-happened dispatch of a product to a customer — the honest,
/// available-today substitute for the Python time-series forecasting service
/// `docs/blueprint/12_AI_PLATFORM.md` §3 describes as ideal-but-out-of-reach
/// (no Python runtime, no training pipeline, no external ML infrastructure
/// exists in this environment). Each row is created 1:1 from Sales'
/// <c>DispatchLineDispatched</c> integration event — a dispatched line means a
/// customer actually took delivery of that quantity, i.e. real demand, not a
/// simulated or fabricated number.
///
/// Deliberately NOT pre-aggregated at write time: one row per dispatched line,
/// storing the raw event's quantity and the date it occurred. Aggregation
/// (grouping same-day rows, summing) happens at forecast-read time in
/// <c>GetDemandForecastQueryHandler</c> — simpler to reason about and more
/// honest about what is actually persisted here (a demand history journal,
/// not a pre-computed daily-summary table).
///
/// <see cref="ProductId"/> and <see cref="WarehouseId"/> are opaque
/// cross-module references, never existence-validated here — same convention
/// as Recommendation.ReferenceId and Quality's Inspection.ReferenceId.
/// </summary>
public sealed class DemandHistoryEntry : TenantAggregateRoot
{
    public Guid ProductId { get; private set; }
    public Guid WarehouseId { get; private set; }
    public decimal QuantityDispatched { get; private set; }

    /// <summary>The date the dispatch occurred (the source event's OccurredOn) — the unit forecast aggregation groups by.</summary>
    public DateTimeOffset PeriodDate { get; private set; }

    private DemandHistoryEntry() { }

    public static DemandHistoryEntry Record(Guid companyId, Guid productId, Guid warehouseId, decimal quantityDispatched, DateTimeOffset periodDate)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (warehouseId == Guid.Empty)
            throw new ArgumentException("Warehouse id is required.", nameof(warehouseId));
        if (quantityDispatched < 0)
            throw new ArgumentException("Quantity dispatched cannot be negative.", nameof(quantityDispatched));

        return new DemandHistoryEntry
        {
            CompanyId = companyId,
            ProductId = productId,
            WarehouseId = warehouseId,
            QuantityDispatched = quantityDispatched,
            PeriodDate = periodDate,
        };
    }
}
