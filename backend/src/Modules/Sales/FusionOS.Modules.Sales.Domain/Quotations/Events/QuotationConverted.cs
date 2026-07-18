using FusionOS.SharedKernel;

namespace FusionOS.Modules.Sales.Domain.Quotations.Events;

/// <summary>
/// Raised once a Quotation has been converted into a real SalesOrder. Has
/// **no consumer this phase** — the natural future hook for a sales-pipeline
/// or quote-conversion-rate report that doesn't exist yet anywhere in this
/// codebase, same deliberate restraint as PickListPacked/
/// GoodsReceiptLinePutAway's unwired state.
/// </summary>
public sealed record QuotationConverted(Guid QuotationId, Guid CompanyId, Guid CustomerId, Guid SalesOrderId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
