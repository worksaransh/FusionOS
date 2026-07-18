namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;

/// <summary>Single place that turns a WorkOrder aggregate into its DTO, shared by every handler that returns one.</summary>
public static class WorkOrderMapper
{
    public static WorkOrderDto ToDto(Domain.WorkOrders.WorkOrder workOrder) => new(
        workOrder.Id,
        workOrder.BillOfMaterialsId,
        workOrder.ProductId,
        workOrder.WarehouseId,
        workOrder.QuantityToProduce,
        workOrder.Status.ToString(),
        workOrder.Components.Select(c => new WorkOrderComponentDto(c.Id, c.ComponentProductId, c.QuantityRequired)).ToList());
}
