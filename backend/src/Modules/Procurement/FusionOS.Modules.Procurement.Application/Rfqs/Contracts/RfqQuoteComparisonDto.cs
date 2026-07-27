namespace FusionOS.Modules.Procurement.Application.Rfqs.Contracts;

/// <summary>
/// One supplier's quoted price for a single RFQ line — absent (not present in the
/// owning <see cref="RfqQuoteComparisonLineDto.Quotes"/> list) when that supplier
/// never quoted this product, rather than a null-filled placeholder. The frontend
/// must look this up by SupplierId per row; it is deliberately sparse so a line only
/// one supplier bid on doesn't fabricate zeros/nulls for the others.
/// </summary>
public sealed record RfqQuoteComparisonCellDto(Guid SupplierId, decimal UnitPrice, decimal LineTotal);

/// <summary>One RFQ line plus every supplier's price against it, for the comparison grid.</summary>
public sealed record RfqQuoteComparisonLineDto(
    Guid RfqLineId,
    Guid ProductId,
    decimal RequestedQuantity,
    IReadOnlyList<RfqQuoteComparisonCellDto> Quotes);

/// <summary>
/// One column header for the comparison grid — a supplier who submitted a quote
/// against this RFQ, with the quote's own id (needed for AwardRfqCommand) and its
/// grand total (the "cheapest overall" glance the frontend sorts/highlights by).
/// </summary>
public sealed record RfqQuoteComparisonSupplierDto(
    Guid SupplierQuoteId,
    Guid SupplierId,
    string? SupplierName,
    decimal TotalAmount);

/// <summary>
/// Comparison-shaped view over an RFQ's SupplierQuotes, for GetRfqQuoteComparisonQuery.
///
/// Shape choice: RFQ Lines are the outer/row axis and Suppliers are the inner/column
/// axis (each line carries a sparse list of per-supplier cells), mirroring
/// RfqDto.Lines being the aggregate's own primary, always-present collection while
/// SupplierQuotes is the variable-cardinality one (zero, one, or many suppliers may
/// have quoted so far). Modelling it any other way — e.g. one row per supplier with
/// a nested list of lines — would force the frontend to invert the data to render
/// "one row per requested product" (the natural decision-making view: "what did
/// everyone quote for product X"), and would need to synthesize placeholder lines
/// for products a given supplier didn't quote. Keeping Lines outermost also matches
/// GetRfqByIdQueryHandler's/CreateRfqCommandHandler's own RfqDto mapping, which
/// always emits Lines before SupplierQuotes.
///
/// Suppliers is still surfaced as its own top-level list (not just inferred from the
/// cells) so the frontend can render column headers and each supplier's grand total
/// — including the "cheapest overall" glance — without scanning every line first.
/// </summary>
public sealed record RfqQuoteComparisonDto(
    Guid RfqId,
    string Status,
    IReadOnlyList<RfqQuoteComparisonSupplierDto> Suppliers,
    IReadOnlyList<RfqQuoteComparisonLineDto> Lines);
