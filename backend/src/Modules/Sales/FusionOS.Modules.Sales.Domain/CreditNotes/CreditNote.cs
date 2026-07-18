using FusionOS.SharedKernel;
using FusionOS.Modules.Sales.Domain.CreditNotes.Events;

namespace FusionOS.Modules.Sales.Domain.CreditNotes;

/// <summary>
/// A return-from-customer document that reverses part or all of an Invoice and,
/// once Issued, posts a credit against the customer's AR balance
/// (docs/IMPLEMENTATION_PLAN.md Phase 10 item 9: "Returns/credit notes"). InvoiceId
/// and CustomerId are real, same-module foreign keys (Invoice and Customer both
/// live in this module), unlike ProductId on the lines. This is the Sales-side
/// commercial document; the actual AR ledger posting from it is Finance's job —
/// see CreditNoteIssued, consumed by CreditNoteIssuedConsumer, mirroring how
/// InvoiceIssued/InvoiceIssuedConsumer post the original charge.
/// </summary>
public sealed class CreditNote : TenantAggregateRoot
{
    private readonly List<CreditNoteLine> _lines = new();

    public Guid InvoiceId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public CreditNoteStatus Status { get; private set; }
    public DateTimeOffset CreditNoteDate { get; private set; }
    public IReadOnlyList<CreditNoteLine> Lines => _lines.AsReadOnly();
    public decimal TotalAmount => _lines.Sum(l => l.LineTotal);

    private CreditNote() { }

    public static CreditNote Create(Guid companyId, Guid invoiceId, Guid customerId, string reason, IReadOnlyCollection<CreditNoteLineInput> lines)
    {
        if (invoiceId == Guid.Empty)
            throw new ArgumentException("Invoice id is required.", nameof(invoiceId));
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer id is required.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A reason is required.", nameof(reason));
        if (lines is null || lines.Count == 0)
            throw new ArgumentException("A credit note must have at least one line.", nameof(lines));

        var creditNote = new CreditNote
        {
            CompanyId = companyId,
            InvoiceId = invoiceId,
            CustomerId = customerId,
            Reason = reason,
            Status = CreditNoteStatus.Draft,
            CreditNoteDate = DateTimeOffset.UtcNow,
        };

        foreach (var line in lines)
            creditNote._lines.Add(CreditNoteLine.Create(line.ProductId, line.Quantity, line.UnitPrice));

        creditNote.Raise(new CreditNoteCreated(creditNote.Id, companyId, invoiceId, customerId, creditNote.TotalAmount));
        return creditNote;
    }

    /// <summary>Raises CreditNoteIssued — the point this credit note is finalized and posted against the customer's AR balance.</summary>
    public void Issue()
    {
        if (Status != CreditNoteStatus.Draft)
            throw new InvalidOperationException($"Only a Draft credit note can be issued (current status: {Status}).");

        Status = CreditNoteStatus.Issued;
        Raise(new CreditNoteIssued(Id, CompanyId, InvoiceId, CustomerId, TotalAmount));
    }
}
