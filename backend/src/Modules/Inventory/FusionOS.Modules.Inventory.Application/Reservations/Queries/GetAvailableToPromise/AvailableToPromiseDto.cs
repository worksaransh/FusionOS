namespace FusionOS.Modules.Inventory.Application.Reservations.Queries.GetAvailableToPromise;

public sealed record AvailableToPromiseDto(Guid ProductId, Guid WarehouseId, decimal StockOnHand, decimal Reserved, decimal Available);
