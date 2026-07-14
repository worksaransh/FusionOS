namespace FusionOS.Modules.Sales.Application.Dispatches.Contracts;

public sealed record DispatchLineDto(Guid Id, Guid ProductId, decimal QuantityDispatched);

public sealed record DispatchDto(
    Guid Id,
    Guid SalesOrderId,
    Guid WarehouseId,
    DateTimeOffset DispatchDate,
    IReadOnlyList<DispatchLineDto> Lines);
