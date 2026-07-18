using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Reservations.Contracts;

namespace FusionOS.Modules.Inventory.Application.Reservations.Commands.CreateReservation;

public sealed record CreateReservationCommand(Guid CompanyId, Guid ProductId, Guid WarehouseId, decimal Quantity, string ReferenceType, Guid ReferenceId)
    : ICommand<ReservationDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.reservation.create" };
    public string EntityType => nameof(Domain.Reservations.Reservation);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
