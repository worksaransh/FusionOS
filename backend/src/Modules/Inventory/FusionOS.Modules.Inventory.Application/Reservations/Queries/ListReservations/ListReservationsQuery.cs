using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Reservations.Contracts;

namespace FusionOS.Modules.Inventory.Application.Reservations.Queries.ListReservations;

public sealed record ListReservationsQuery(Guid CompanyId, Guid? ProductId = null, Guid? WarehouseId = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<ReservationDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "inventory.reservation.read" };
}
