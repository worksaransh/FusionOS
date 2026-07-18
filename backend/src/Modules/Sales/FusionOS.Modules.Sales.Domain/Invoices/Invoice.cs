using FusionOS.SharedKernel;
using FusionOS.Modules.Sales.Domain.Invoices.Events;

namespace FusionOS.Modules.Sales.Domain.Invoices;

/// <summary>
/// The billing document for a Sales Order (05_MODULE_ROADMAP.md Phase 1: Sales
/// capability list — "Invoice"). SalesOrderId and CustomerId are real, same-module
/// foreign keys (unlike ProductId on the lines), since SalesOrder and Customer
/// both live in this module. This is the Sales-side commercial document; the
/// actual General Ledger/Accounts Receivable posting from it is Finance's job
/// (05_MODULE_ROADMAP.md Phase 2) and is not built yet — see InvoiceIssued.
/// </summary>
public sealed class Invoice : TenantAggregateRoot
{
    private readonly List<InvoiceLine> _lines = new();

    public Guid SalesOrderId { get; private set; }
    public Guid CustomerId { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public DateTimeOffset InvoiceDate { get; private set; }
    public IReadOnlyList<InvoiceLine> Lines => _lines.AsReadOnly();
    public decimal TotalAmount => _lines.Sum(l => l.LineTotal);

    /// <summary>
    /// Optional cross-module reference to Core's User (opaque, never
    /// existence-validated — same convention as ProductId on the lines, since
    /// validating it would require a Sales→Core project reference this module
    /// doesn't otherwise take). Set at creation, feeding the sales-commission
    /// summary report (docs/IMPLEMENTATION_PLAN.md Phase 10 item 11) — commission
    /// is computed on invoiced, not just ordered, revenue, which is why this
    /// lives on Invoice rather than SalesOrder.
    /// </summary>
    public Guid? SalesPersonId { get; private set; }

    private Invoice() { }

    public static Invoice Create(Guid companyId, Guid salesOrderId, Guid customerId, IReadOnlyCollection<InvoiceLineInput> lines, Guid? salesPersonId = null)
    {
        if (salesOrderId == Guid.Empty)
            throw new ArgumentException("Sales order id is required.", nameof(salesOrderId));
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer id is required.", nameof(customerId));
        if (lines is null || lines.Count == 0)
            throw new ArgumentException("An invoice must have at least one line.", nameof(lines));

        var invoice = new Invoice
        {
            CompanyId = companyId,
            SalesOrderId = salesOrderId,
            CustomerId = customerId,
            Status = InvoiceStatus.Draft,
            InvoiceDate = DateTimeOffset.UtcNow,
            SalesPersonId = salesPersonId,
        };

        foreach (var line in lines)
            invoice._lines.Add(InvoiceLine.Create(line.ProductId, line.Quantity, line.UnitPrice, line.TaxRateId, line.TaxAmount));

        invoice.Raise(new InvoiceCreated(invoice.Id, companyId, salesOrderId, customerId, invoice.TotalAmount));
        return invoice;
    }

    /// <summary>Raises InvoiceIssued — the point this invoice is finalized and sent to the customer.</summary>
    public void Issue()
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException($"Only a Draft invoice can be issued (current status: {Status}).");

        Status = InvoiceStatus.Issued;
        Raise(new InvoiceIssued(Id, CompanyId, SalesOrderId, CustomerId, TotalAmount));
    }
}
