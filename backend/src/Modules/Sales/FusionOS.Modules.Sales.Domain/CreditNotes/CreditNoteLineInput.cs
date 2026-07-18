namespace FusionOS.Modules.Sales.Domain.CreditNotes;

public sealed record CreditNoteLineInput(Guid ProductId, decimal Quantity, decimal UnitPrice);
