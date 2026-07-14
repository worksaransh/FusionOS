namespace FusionOS.Modules.Warehouse.Application.Zones.Contracts;

public sealed record ZoneDto(Guid Id, Guid WarehouseId, string Name, string Code, bool IsActive, DateTimeOffset CreatedAt);
