namespace FusionOS.Modules.Warehouse.Application.Bins.Contracts;

/// <summary>ShelfId is the bin's optional, additive location refinement (Bin.ShelfId) — null unless AssignBinShelfCommand has set it.</summary>
public sealed record BinDto(Guid Id, Guid ZoneId, string Name, string Code, bool IsActive, DateTimeOffset CreatedAt, Guid? ShelfId = null);
