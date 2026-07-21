namespace FusionOS.Modules.Warehouse.Application.Racks.Contracts;

public sealed record RackDto(Guid Id, Guid ZoneId, string Name, string Code, bool IsActive, DateTimeOffset CreatedAt);
