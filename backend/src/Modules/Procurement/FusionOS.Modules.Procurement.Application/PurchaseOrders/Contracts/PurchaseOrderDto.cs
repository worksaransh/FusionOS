namespace FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;

public sealed record PurchaseOrderLineDto(Guid Id, Guid ProductId, decimal Quantity, decimal UnitPrice, decimal LineTotal, decimal ReceivedQuantity);

public sealed record PurchaseOrderDto(Guid Id, Guid SupplierId, string Status, DateTimeOffset OrderDate, decimal TotalAmount, IReadOnlyList<PurchaseOrderLineDto> Lines);
