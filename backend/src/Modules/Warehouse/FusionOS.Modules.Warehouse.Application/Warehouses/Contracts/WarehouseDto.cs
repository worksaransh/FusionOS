namespace FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;

public sealed record WarehouseDto(Guid Id, string Name, string Code, string? Address, bool IsActive, DateTimeOffset CreatedAt);
