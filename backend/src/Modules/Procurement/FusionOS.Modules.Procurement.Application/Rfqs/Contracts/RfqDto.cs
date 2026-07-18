namespace FusionOS.Modules.Procurement.Application.Rfqs.Contracts;

public sealed record RfqLineDto(Guid Id, Guid ProductId, decimal Quantity);

public sealed record SupplierQuoteLineDto(Guid Id, Guid ProductId, decimal Quantity, decimal UnitPrice, decimal LineTotal);

public sealed record SupplierQuoteDto(Guid Id, Guid SupplierId, DateTimeOffset SubmittedAt, decimal TotalAmount, IReadOnlyList<SupplierQuoteLineDto> Lines);

public sealed record RfqDto(
    Guid Id,
    string Status,
    DateTimeOffset RfqDate,
    Guid? AwardedSupplierQuoteId,
    Guid? ConvertedPurchaseOrderId,
    IReadOnlyList<RfqLineDto> Lines,
    IReadOnlyList<SupplierQuoteDto> SupplierQuotes);
