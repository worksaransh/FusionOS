namespace FusionOS.Modules.Warehouse.Application.Shelves.Contracts;

public sealed record ShelfDto(Guid Id, Guid RackId, string Name, string Code, bool IsActive, DateTimeOffset CreatedAt);
