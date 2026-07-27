using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.Returns.Events;

/// <summary>
/// Deliberately carries no CreditNote linkage — Complete() only records that the
/// physical goods arrived back and were inspected/restocked-or-not; whether a credit
/// note is ever issued for this return is a separate, manual, user-triggered action
/// (see CreditNote.SalesReturnId), same "no automatic cross-aggregate cascade"
/// convention as Invoice/CreditNote themselves.
/// </summary>
public sealed record SalesReturnCompleted(Guid SalesReturnId, Guid CompanyId, Guid InvoiceId, Guid CustomerId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
