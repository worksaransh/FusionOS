using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Reservations.Contracts;

namespace FusionOS.Modules.Inventory.Application.Reservations.Commands.FulfillReservation;

public sealed record FulfillReservationCommand(Guid CompanyId, Guid ReservationId)
    : ICommand<ReservationDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.reservation.fulfill" };
    public string EntityType => nameof(Domain.Reservations.Reservation);
    public Guid EntityId => ReservationId;
    public string Action => "Fulfilled";
}
