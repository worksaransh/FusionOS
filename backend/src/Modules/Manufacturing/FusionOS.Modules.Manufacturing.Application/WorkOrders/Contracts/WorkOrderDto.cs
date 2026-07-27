namespace FusionOS.Modules.Manufacturing.Application.WorkOrders.Contracts;

public sealed record WorkOrderComponentDto(Guid Id, Guid ComponentProductId, decimal QuantityRequired, decimal QuantityIssued);

/// <summary>One of this work order's snapshotted routing operations (Phase 6, 2026-07-22) — see WorkOrder.Operations.</summary>
public sealed record WorkOrderOperationDto(
    Guid Id,
    Guid RoutingOperationId,
    int SequenceNumber,
    string OperationName,
    string WorkCenter,
    decimal StandardMinutes,
    string Status,
    Guid? MachineId,
    DateTimeOffset? ScheduledStart,
    DateTimeOffset? ScheduledEnd,
    DateTimeOffset? ActualStart,
    DateTimeOffset? ActualEnd);

public sealed record WorkOrderDto(
    Guid Id,
    Guid BillOfMaterialsId,
    Guid ProductId,
    Guid WarehouseId,
    decimal QuantityToProduce,
    string Status,
    IReadOnlyList<WorkOrderComponentDto> Components,
    decimal? QuantityGoodProduced,
    decimal QuantityScrapped,
    decimal? YieldPercentage,
    IReadOnlyList<WorkOrderOperationDto> Operations,
    Guid? SourceWorkOrderId,
    decimal QuantityReworked);
