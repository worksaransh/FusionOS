namespace FusionOS.Modules.Warehouse.Application.CycleCounts.Contracts;

public sealed record CycleCountDto(
    Guid Id,
    Guid WarehouseId,
    Guid ZoneId,
    Guid BinId,
    Guid ProductId,
    Guid StartedBy,
    decimal SystemQuantitySnapshot,
    decimal? CountedQuantity,
    decimal? VarianceQuantity,
    string Status,
    DateTimeOffset CreatedAt);
