using FusionOS.SharedKernel;
using FusionOS.Modules.Sales.Domain.Returns.Events;

namespace FusionOS.Modules.Sales.Domain.Returns;

/// <summary>
/// The physical-return/RMA workflow this codebase was missing: a customer's REQUEST
/// to send goods back against an Invoice, its approval/rejection, and — once the goods
/// physically arrive and are inspected — its completion (with per-line condition notes
/// and whether each line was restocked). InvoiceId and CustomerId are real, same-module
/// foreign keys (Invoice and Customer both live in this module), unlike ProductId on
/// the lines — same convention as CreditNote.
///
/// This is a deliberately DISTINCT concern from CreditNote (the financial/AR document):
/// a SalesReturn can be Rejected with no CreditNote ever created, or Completed and only
/// later (and optionally, and manually — see CreditNote.SalesReturnId) followed by a
/// credit note for part of the returned value. Approved is a distinct state from
/// Completed: Approved means "we will accept this return"; Completed means "the goods
/// physically arrived back and were inspected/restocked" — those happen at different
/// times and are recorded by different actors (an approver vs. warehouse/inspection
/// staff), which is why they are not conflated into a single transition.
/// </summary>
public sealed class SalesReturn : TenantAggregateRoot
{
    private readonly List<SalesReturnLine> _lines = new();

    public Guid InvoiceId { get; private set; }
    public Guid CustomerId { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public SalesReturnStatus Status { get; private set; }
    public DateTimeOffset RequestDate { get; private set; }
    public string? RejectionReason { get; private set; }
    public IReadOnlyList<SalesReturnLine> Lines => _lines.AsReadOnly();

    private SalesReturn() { }

    public static SalesReturn Create(Guid companyId, Guid invoiceId, Guid customerId, string reason, IReadOnlyCollection<SalesReturnLineInput> lines)
    {
        if (invoiceId == Guid.Empty)
            throw new ArgumentException("Invoice id is required.", nameof(invoiceId));
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer id is required.", nameof(customerId));
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A reason is required.", nameof(reason));
        if (lines is null || lines.Count == 0)
            throw new ArgumentException("A sales return must have at least one line.", nameof(lines));

        var salesReturn = new SalesReturn
        {
            CompanyId = companyId,
            InvoiceId = invoiceId,
            CustomerId = customerId,
            Reason = reason,
            Status = SalesReturnStatus.Requested,
            RequestDate = DateTimeOffset.UtcNow,
        };

        foreach (var line in lines)
            salesReturn._lines.Add(SalesReturnLine.Create(line.ProductId, line.Quantity));

        salesReturn.Raise(new SalesReturnRequested(salesReturn.Id, companyId, invoiceId, customerId, reason));
        return salesReturn;
    }

    /// <summary>Requested -> Approved: "we will accept this return" — does not yet mean the goods have arrived back.</summary>
    public void Approve()
    {
        if (Status != SalesReturnStatus.Requested)
            throw new InvalidOperationException($"Only a Requested sales return can be approved (current status: {Status}).");

        Status = SalesReturnStatus.Approved;
        Raise(new SalesReturnApproved(Id, CompanyId, InvoiceId, CustomerId));
    }

    /// <summary>Requested -> Rejected. A reason is required for the audit trail — same convention as VendorReturn/CreditNote requiring one at creation.</summary>
    public void Reject(string rejectionReason)
    {
        if (Status != SalesReturnStatus.Requested)
            throw new InvalidOperationException($"Only a Requested sales return can be rejected (current status: {Status}).");
        if (string.IsNullOrWhiteSpace(rejectionReason))
            throw new ArgumentException("A rejection reason is required.", nameof(rejectionReason));

        Status = SalesReturnStatus.Rejected;
        RejectionReason = rejectionReason;
        Raise(new SalesReturnRejected(Id, CompanyId, InvoiceId, CustomerId, rejectionReason));
    }

    /// <summary>
    /// Approved -> Completed: the goods physically arrived back and were inspected.
    /// Every line must be covered by exactly one completion — partial completion isn't
    /// modeled (mirrors CreditNote/Invoice's "the whole document moves as one unit"
    /// state-machine shape); a return with lines still uninspected simply isn't
    /// Completed yet.
    /// </summary>
    public void Complete(IReadOnlyCollection<SalesReturnLineCompletion> lineCompletions)
    {
        if (Status != SalesReturnStatus.Approved)
            throw new InvalidOperationException($"Only an Approved sales return can be completed (current status: {Status}).");
        if (lineCompletions is null || lineCompletions.Count != _lines.Count)
            throw new ArgumentException("Every line of the sales return must have exactly one completion entry.", nameof(lineCompletions));

        var completionsByLineId = new Dictionary<Guid, SalesReturnLineCompletion>();
        foreach (var completion in lineCompletions)
        {
            if (!completionsByLineId.TryAdd(completion.LineId, completion))
                throw new ArgumentException($"Line {completion.LineId} has more than one completion entry.", nameof(lineCompletions));
        }

        foreach (var line in _lines)
        {
            if (!completionsByLineId.TryGetValue(line.Id, out var completion))
                throw new ArgumentException($"Line {line.Id} has no completion entry.", nameof(lineCompletions));

            line.ApplyCompletion(completion.Restocked, completion.ConditionNotes);
        }

        Status = SalesReturnStatus.Completed;
        Raise(new SalesReturnCompleted(Id, CompanyId, InvoiceId, CustomerId));
    }
}
