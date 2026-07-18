using FusionOS.SharedKernel;
using FusionOS.Modules.Sales.Domain.Quotations.Events;

namespace FusionOS.Modules.Sales.Domain.Quotations;

/// <summary>
/// A pre-Sales-Order stage, convertible into a real Sales Order once accepted
/// (docs/IMPLEMENTATION_PLAN.md Phase 10 item 8) — named in SalesOrder's own
/// doc comment as coming "later." CustomerId is a real, same-module foreign
/// key (Customer lives in this module), validated the same way
/// CreateSalesOrderCommandHandler validates it via ICustomerRepository.
/// ExistsAsync, unlike the opaque cross-module ProductId on the lines.
///
/// Lifecycle is Draft → Accepted|Rejected, then Accepted → Converted. Unlike
/// Invoice/CreditNote/SalesOrder's simple Draft→Issued/Confirmed two-state
/// lifecycle, a rejected quotation is a real terminal outcome worth recording
/// (a sales team wants to know how many quotes were lost, not just delete
/// them) rather than silently discarding the row.
/// </summary>
public sealed class Quotation : TenantAggregateRoot
{
    private readonly List<QuotationLine> _lines = new();

    public Guid CustomerId { get; private set; }
    public QuotationStatus Status { get; private set; }
    public DateTimeOffset QuotationDate { get; private set; }
    public Guid? ConvertedSalesOrderId { get; private set; }
    public IReadOnlyList<QuotationLine> Lines => _lines.AsReadOnly();
    public decimal TotalAmount => _lines.Sum(l => l.LineTotal);

    private Quotation() { }

    public static Quotation Create(Guid companyId, Guid customerId, IReadOnlyCollection<QuotationLineInput> lines)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer id is required.", nameof(customerId));
        if (lines is null || lines.Count == 0)
            throw new ArgumentException("A quotation must have at least one line.", nameof(lines));

        var quotation = new Quotation
        {
            CompanyId = companyId,
            CustomerId = customerId,
            Status = QuotationStatus.Draft,
            QuotationDate = DateTimeOffset.UtcNow,
        };

        foreach (var line in lines)
            quotation._lines.Add(QuotationLine.Create(line.ProductId, line.Quantity, line.UnitPrice));

        quotation.Raise(new QuotationCreated(quotation.Id, companyId, customerId, quotation.TotalAmount));
        return quotation;
    }

    /// <summary>Marks the quotation as accepted by the customer — the only status a conversion into a SalesOrder can start from.</summary>
    public void Accept()
    {
        if (Status != QuotationStatus.Draft)
            throw new InvalidOperationException($"Only a Draft quotation can be accepted (current status: {Status}).");

        Status = QuotationStatus.Accepted;
        Raise(new QuotationAccepted(Id, CompanyId, CustomerId, TotalAmount));
    }

    /// <summary>Marks the quotation as rejected — a real terminal outcome, not a delete.</summary>
    public void Reject()
    {
        if (Status != QuotationStatus.Draft)
            throw new InvalidOperationException($"Only a Draft quotation can be rejected (current status: {Status}).");

        Status = QuotationStatus.Rejected;
        Raise(new QuotationRejected(Id, CompanyId, CustomerId));
    }

    /// <summary>
    /// Records that this quotation was converted into the given SalesOrder.
    /// The SalesOrder itself is created by ConvertQuotationToSalesOrderCommandHandler
    /// (same module, same DbContext — no cross-module event needed, same
    /// restraint as the Approval engine creating a Notification row directly);
    /// this method only updates the Quotation's own side of that fact.
    /// </summary>
    public void MarkConverted(Guid salesOrderId)
    {
        if (Status != QuotationStatus.Accepted)
            throw new InvalidOperationException($"Only an Accepted quotation can be converted (current status: {Status}).");
        if (salesOrderId == Guid.Empty)
            throw new ArgumentException("Sales order id is required.", nameof(salesOrderId));

        Status = QuotationStatus.Converted;
        ConvertedSalesOrderId = salesOrderId;
        Raise(new QuotationConverted(Id, CompanyId, CustomerId, salesOrderId));
    }
}
