namespace FusionOS.Modules.Procurement.Domain.Rfqs;

/// <summary>
/// One supplier's response to an RFQ — the "multiple suppliers are asked to quote"
/// half of docs/IMPLEMENTATION_PLAN.md Phase 10 item 1 (the other half, awarding and
/// converting the winner into a real PurchaseOrder, is RequestForQuotation.Award/
/// ConvertRfqToPurchaseOrderCommandHandler). Owned entirely by the parent
/// RequestForQuotation — same documented exception to per-row audit/tenant columns
/// as every other line/child entity in this codebase (04_DATABASE_GUIDELINES.md §3).
/// SupplierId is a real, same-module foreign key (Supplier lives in this module),
/// validated by SubmitSupplierQuoteCommandHandler via ISupplierRepository.ExistsAsync
/// — unlike the opaque cross-module ProductId on each SupplierQuoteLine.
/// </summary>
public sealed class SupplierQuote
{
    private readonly List<SupplierQuoteLine> _lines = new();

    public Guid Id { get; private set; }
    public Guid SupplierId { get; private set; }
    public DateTimeOffset SubmittedAt { get; private set; }
    public IReadOnlyList<SupplierQuoteLine> Lines => _lines.AsReadOnly();
    public decimal TotalAmount => _lines.Sum(l => l.LineTotal);

    private SupplierQuote() { }

    internal static SupplierQuote Create(Guid supplierId, IReadOnlyCollection<SupplierQuoteLineInput> lines)
    {
        if (supplierId == Guid.Empty)
            throw new ArgumentException("Supplier id is required.", nameof(supplierId));
        if (lines is null || lines.Count == 0)
            throw new ArgumentException("A supplier quote must have at least one line.", nameof(lines));

        var quote = new SupplierQuote
        {
            Id = Guid.NewGuid(),
            SupplierId = supplierId,
            SubmittedAt = DateTimeOffset.UtcNow,
        };

        foreach (var line in lines)
            quote._lines.Add(SupplierQuoteLine.Create(line.ProductId, line.Quantity, line.UnitPrice));

        return quote;
    }
}
