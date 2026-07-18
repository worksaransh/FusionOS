using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.CreditNotes.Events;

public sealed record CreditNoteCreated(Guid CreditNoteId, Guid CompanyId, Guid InvoiceId, Guid CustomerId, decimal TotalAmount) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
