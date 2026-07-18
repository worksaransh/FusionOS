using FusionOS.SharedKernel;
using FusionOS.Modules.Procurement.Domain.Rfqs.Events;

namespace FusionOS.Modules.Procurement.Domain.Rfqs;

/// <summary>
/// A pre-PO stage where multiple suppliers are asked to quote; the winning quote
/// converts into a real PurchaseOrder (docs/IMPLEMENTATION_PLAN.md Phase 10 item 1)
/// — named in PurchaseOrder's own doc comment as coming "later." Structurally the
/// closest analogue to Quotation→SalesOrder (same Create→award→convert shape, same
/// "aggregate creates aggregate in the same DbContext, single SaveChanges" pattern
/// for the conversion step), with one added wrinkle that is the entire point of an
/// RFQ versus a Quotation: instead of a single set of terms, multiple SupplierQuote
/// candidates are collected before one is picked.
///
/// Lifecycle is Draft → Sent → Awarded, then a separate ConvertRfqToPurchaseOrder
/// step (mirroring Quotation's own separate Accept-then-Convert split) moves the
/// awarded quote into a real PurchaseOrder. Only Sent RFQs accept SupplierQuotes —
/// a supplier cannot quote against something never sent to them.
/// </summary>
public sealed class RequestForQuotation : TenantAggregateRoot
{
    private readonly List<RfqLine> _lines = new();
    private readonly List<SupplierQuote> _supplierQuotes = new();

    public RfqStatus Status { get; private set; }
    public DateTimeOffset RfqDate { get; private set; }
    public Guid? AwardedSupplierQuoteId { get; private set; }
    public Guid? ConvertedPurchaseOrderId { get; private set; }
    public IReadOnlyList<RfqLine> Lines => _lines.AsReadOnly();
    public IReadOnlyList<SupplierQuote> SupplierQuotes => _supplierQuotes.AsReadOnly();

    private RequestForQuotation() { }

    public static RequestForQuotation Create(Guid companyId, IReadOnlyCollection<RfqLineInput> lines)
    {
        if (lines is null || lines.Count == 0)
            throw new ArgumentException("An RFQ must have at least one line.", nameof(lines));

        var rfq = new RequestForQuotation
        {
            CompanyId = companyId,
            Status = RfqStatus.Draft,
            RfqDate = DateTimeOffset.UtcNow,
        };

        foreach (var line in lines)
            rfq._lines.Add(RfqLine.Create(line.ProductId, line.Quantity));

        rfq.Raise(new RfqCreated(rfq.Id, companyId, rfq._lines.Count));
        return rfq;
    }

    /// <summary>Marks the RFQ as sent to suppliers — the only status SupplierQuotes can be submitted against.</summary>
    public void Send()
    {
        if (Status != RfqStatus.Draft)
            throw new InvalidOperationException($"Only a Draft RFQ can be sent (current status: {Status}).");

        Status = RfqStatus.Sent;
        Raise(new RfqSent(Id, CompanyId));
    }

    /// <summary>
    /// Records one supplier's quote against this RFQ. Each product quoted must be
    /// one this RFQ actually asked about (validated against this aggregate's own
    /// Lines — no cross-module read needed).
    ///
    /// A supplier may re-submit: if this supplier already has a quote on this RFQ,
    /// the prior one is replaced wholesale by the new submission (suppliers routinely
    /// revise a quote before an award is made). Because SubmitSupplierQuote is only
    /// permitted while the RFQ is still Sent — Award moves it to Awarded, after which
    /// no further submissions are accepted — a replacement can only ever happen before
    /// any quote has been awarded, so there is no awarded-quote reference to
    /// invalidate. The resubmission still raises SupplierQuoteSubmitted (it is a
    /// submission); no separate "quote replaced" event is modelled.
    /// </summary>
    public SupplierQuote SubmitSupplierQuote(Guid supplierId, IReadOnlyCollection<SupplierQuoteLineInput> lines)
    {
        if (Status != RfqStatus.Sent)
            throw new InvalidOperationException($"Only a Sent RFQ can receive supplier quotes (current status: {Status}).");
        if (lines is null || lines.Count == 0)
            throw new ArgumentException("A supplier quote must have at least one line.", nameof(lines));

        var requestedProductIds = _lines.Select(l => l.ProductId).ToHashSet();
        foreach (var line in lines)
        {
            if (!requestedProductIds.Contains(line.ProductId))
                throw new ArgumentException($"Product '{line.ProductId}' was not part of this RFQ.", nameof(lines));
        }

        // Resubmission replaces the supplier's prior quote rather than being rejected.
        var existing = _supplierQuotes.FirstOrDefault(q => q.SupplierId == supplierId);
        if (existing is not null)
            _supplierQuotes.Remove(existing);

        var quote = SupplierQuote.Create(supplierId, lines);
        _supplierQuotes.Add(quote);
        Raise(new SupplierQuoteSubmitted(Id, CompanyId, quote.Id, supplierId, quote.TotalAmount));
        return quote;
    }

    /// <summary>Picks the winning SupplierQuote — the only status a conversion into a PurchaseOrder can start from.</summary>
    public void Award(Guid supplierQuoteId)
    {
        if (Status != RfqStatus.Sent)
            throw new InvalidOperationException($"Only a Sent RFQ can be awarded (current status: {Status}).");

        var quote = _supplierQuotes.FirstOrDefault(q => q.Id == supplierQuoteId)
            ?? throw new ArgumentException($"Supplier quote '{supplierQuoteId}' was not submitted against this RFQ.", nameof(supplierQuoteId));

        Status = RfqStatus.Awarded;
        AwardedSupplierQuoteId = quote.Id;
        Raise(new RfqAwarded(Id, CompanyId, quote.Id, quote.SupplierId));
    }

    /// <summary>
    /// Records that this RFQ was converted into the given PurchaseOrder. The
    /// PurchaseOrder itself is created by ConvertRfqToPurchaseOrderCommandHandler
    /// (same module, same DbContext — no cross-module event needed, same restraint
    /// as ConvertQuotationToSalesOrderCommandHandler in Sales); this method only
    /// updates the Rfq's own side of that fact.
    /// </summary>
    public void MarkConverted(Guid purchaseOrderId)
    {
        if (Status != RfqStatus.Awarded)
            throw new InvalidOperationException($"Only an Awarded RFQ can be converted (current status: {Status}).");
        if (ConvertedPurchaseOrderId is not null)
            throw new InvalidOperationException("This RFQ has already been converted into a purchase order.");
        if (purchaseOrderId == Guid.Empty)
            throw new ArgumentException("Purchase order id is required.", nameof(purchaseOrderId));

        var awardedQuote = _supplierQuotes.First(q => q.Id == AwardedSupplierQuoteId);
        ConvertedPurchaseOrderId = purchaseOrderId;
        Raise(new RfqConverted(Id, CompanyId, awardedQuote.SupplierId, purchaseOrderId));
    }
}
