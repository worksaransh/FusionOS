namespace FusionOS.Modules.Sales.Application.CreditNotes.Contracts;

public sealed record CreditNoteLineDto(Guid Id, Guid ProductId, decimal Quantity, decimal UnitPrice, decimal LineTotal);

public sealed record CreditNoteDto(
    Guid Id,
    Guid InvoiceId,
    Guid CustomerId,
    string Reason,
    string Status,
    DateTimeOffset CreditNoteDate,
    decimal TotalAmount,
    IReadOnlyList<CreditNoteLineDto> Lines);
