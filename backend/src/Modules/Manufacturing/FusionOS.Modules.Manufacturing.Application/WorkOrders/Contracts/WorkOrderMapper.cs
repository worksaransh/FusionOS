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
        workOrder.Components.Select(c => new WorkOrderComponentDto(c.Id, c.ComponentProductId, c.QuantityRequired, c.QuantityIssued)).ToList(),
        workOrder.QuantityGoodProduced,
        workOrder.QuantityScrapped,
        workOrder.YieldPercentage,
        workOrder.Operations.Select(o => new WorkOrderOperationDto(
            o.Id,
            o.RoutingOperationId,
            o.SequenceNumber,
            o.OperationName,
            o.WorkCenter,
            o.StandardMinutes,
            o.Status.ToString(),
            o.MachineId,
            o.ScheduledStart,
            o.ScheduledEnd,
            o.ActualStart,
            o.ActualEnd)).ToList(),
        workOrder.SourceWorkOrderId,
        workOrder.QuantityReworked);
}
