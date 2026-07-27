namespace FusionOS.Modules.Sales.Application.Returns.Contracts;

public sealed record SalesReturnLineDto(Guid Id, Guid ProductId, decimal Quantity, string? ConditionNotes, bool Restocked);

public sealed record SalesReturnDto(
    Guid Id,
    Guid InvoiceId,
    Guid CustomerId,
    string Reason,
    string Status,
    DateTimeOffset RequestDate,
    string? RejectionReason,
    IReadOnlyList<SalesReturnLineDto> Lines);
