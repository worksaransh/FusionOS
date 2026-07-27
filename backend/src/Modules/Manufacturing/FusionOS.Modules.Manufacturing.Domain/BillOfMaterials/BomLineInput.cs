namespace FusionOS.Modules.Manufacturing.Domain.BillOfMaterials;

/// <summary>
/// Input shape for one BOM component line — kept in Domain so both Application and tests
/// reference one definition. UnitCost (WIP support, Phase 6, 2026-07-22) is optional and
/// caller-supplied, same convention as Finance's TaxRate.TaxAmount: this module has no
/// reachable Inventory product-cost lookup (no cross-module query, only opaque
/// ProductId references), so the cost a work order's WIP value uses is whatever the
/// caller provides at BOM authoring time, defaulting to 0 when omitted.
/// </summary>
public sealed record BomLineInput(Guid ComponentProductId, decimal Quantity, decimal UnitCost = 0m);
