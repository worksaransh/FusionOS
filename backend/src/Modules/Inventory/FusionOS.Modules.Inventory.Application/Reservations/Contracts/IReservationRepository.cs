namespace FusionOS.Modules.Inventory.Application.Reservations.Contracts;

public interface IReservationRepository
{
    Task<Domain.Reservations.Reservation?> GetByIdAsync(Guid companyId, Guid reservationId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Reservations.Reservation reservation, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Reservations.Reservation>> ListAsync(Guid companyId, Guid? productId, Guid? warehouseId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid? productId, Guid? warehouseId, CancellationToken cancellationToken = default);

    /// <summary>Sum of every Active reservation's Quantity for this Product/Warehouse — the "held" half of available-to-promise (GetAvailableToPromiseQuery).</summary>
    Task<decimal> SumActiveQuantityAsync(Guid companyId, Guid productId, Guid warehouseId, CancellationToken cancellationToken = default);
}
