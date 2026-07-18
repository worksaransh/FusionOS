namespace FusionOS.Modules.Inventory.Application.Reports.Contracts;

/// <summary>
/// One product's weighted-average-cost (WAC) valuation, computed by folding its full
/// ledger history through WeightedAverageCostCalculator (M9 remaining — Inventory
/// costing, 2026-07-16). Company-wide per product (summed across warehouses), matching
/// the same scope as the existing last-cost-based StockValuationReportDto (Phase M6) —
/// this report supersedes that one's valuation number with a real weighted average
/// instead of "most recent cost," and additionally surfaces cumulative COGS.
/// </summary>
public sealed record InventoryValuationLineDto(
    Guid ProductId,
    string Sku,
    string Name,
    decimal OnHandQuantity,
    decimal WeightedAverageUnitCost,
    decimal TotalValuation,
    decimal CumulativeCostOfGoodsSold);

public sealed record InventoryValuationReportDto(
    IReadOnlyList<InventoryValuationLineDto> Lines,
    decimal GrandTotalValuation,
    decimal GrandTotalCostOfGoodsSold);
