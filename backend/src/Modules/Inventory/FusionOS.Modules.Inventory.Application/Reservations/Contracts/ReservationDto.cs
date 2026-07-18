namespace FusionOS.Modules.Inventory.Application.Reservations.Contracts;

public sealed record ReservationDto(Guid Id, Guid ProductId, Guid WarehouseId, decimal Quantity, string ReferenceType, Guid ReferenceId, string Status);

/// <summary>Single place that turns a Reservation aggregate into its DTO, shared by every handler that returns one.</summary>
public static class ReservationMapper
{
    public static ReservationDto ToDto(Domain.Reservations.Reservation reservation) => new(
        reservation.Id, reservation.ProductId, reservation.WarehouseId, reservation.Quantity,
        reservation.ReferenceType, reservation.ReferenceId, reservation.Status.ToString());
}
