using FusionOS.BuildingBlocks.Application.Abstractions;

namespace FusionOS.Modules.Inventory.Application.Reservations.Queries.GetAvailableToPromise;

/// <summary>Available = StockOnHand - sum(Active reservations). The frontend (or a future Sales-side check) calls this before promising a quantity on a new order line — see Reservation's own class doc comment for why this composes two existing sources rather than a new cached balance.</summary>
public sealed record GetAvailableToPromiseQuery(Guid CompanyId, Guid ProductId, Guid WarehouseId) : IQuery<AvailableToPromiseDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "inventory.reservation.read" };
}
