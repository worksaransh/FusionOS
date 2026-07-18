namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;

public sealed record WorkOrderComponentDto(Guid Id, Guid ComponentProductId, decimal QuantityRequired);

public sealed record WorkOrderDto(
    Guid Id,
    Guid BillOfMaterialsId,
    Guid ProductId,
    Guid WarehouseId,
    decimal QuantityToProduce,
    string Status,
    IReadOnlyList<WorkOrderComponentDto> Components);
