namespace FusionOS.Modules.Inventory.Application.Reports.Contracts;

/// <summary>One product's on-hand quantity valued at its most recent recorded unit cost (Phase M6, 2026-07-15).</summary>
public sealed record StockValuationLineDto(Guid ProductId, string Sku, string Name, decimal OnHandQuantity, decimal? LastUnitCost, decimal ExtendedValue);

public sealed record StockValuationReportDto(IReadOnlyList<StockValuationLineDto> Lines, decimal GrandTotalValue);
