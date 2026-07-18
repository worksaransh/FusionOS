namespace FusionOS.Modules.Warehouse.Application.PickLists.Contracts;

public sealed record PickListLineDto(Guid Id, Guid ProductId, Guid? BinId, decimal QuantityToPick, decimal QuantityPicked);

public sealed record PickListDto(
    Guid Id,
    Guid WarehouseId,
    Guid SalesOrderId,
    Guid? AssignedToUserId,
    string Status,
    IReadOnlyList<PickListLineDto> Lines,
    DateTimeOffset CreatedAt);
