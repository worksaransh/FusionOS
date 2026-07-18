namespace FusionOS.Modules.Warehouse.Application.Bins.Contracts;

public sealed record BinDto(Guid Id, Guid ZoneId, string Name, string Code, bool IsActive, DateTimeOffset CreatedAt);
