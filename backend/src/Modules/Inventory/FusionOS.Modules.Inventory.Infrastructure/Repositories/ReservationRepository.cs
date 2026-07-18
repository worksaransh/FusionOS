using FusionOS.Modules.Inventory.Application.Reservations.Contracts;
using FusionOS.Modules.Inventory.Domain.Reservations;
using FusionOS.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Inventory.Infrastructure.Repositories;

public sealed class ReservationRepository : IReservationRepository
{
    private readonly InventoryDbContext _context;

    public ReservationRepository(InventoryDbContext context) => _context = context;

    public Task<Reservation?> GetByIdAsync(Guid companyId, Guid reservationId, CancellationToken cancellationToken = default) =>
        _context.Reservations.FirstOrDefaultAsync(r => r.CompanyId == companyId && r.Id == reservationId, cancellationToken);

    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default) =>
        await _context.Reservations.AddAsync(reservation, cancellationToken);

    public async Task<IReadOnlyList<Reservation>> ListAsync(Guid companyId, Guid? productId, Guid? warehouseId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, productId, warehouseId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid? productId, Guid? warehouseId, CancellationToken cancellationToken = default) =>
        Filtered(companyId, productId, warehouseId).CountAsync(cancellationToken);

    public Task<decimal> SumActiveQuantityAsync(Guid companyId, Guid productId, Guid warehouseId, CancellationToken cancellationToken = default) =>
        _context.Reservations
            .Where(r => r.CompanyId == companyId && r.ProductId == productId && r.WarehouseId == warehouseId && r.Status == ReservationStatus.Active)
            .SumAsync(r => r.Quantity, cancellationToken);

    private IQueryable<Reservation> Filtered(Guid companyId, Guid? productId, Guid? warehouseId)
    {
        var query = _context.Reservations.Where(r => r.CompanyId == companyId);
        if (productId.HasValue)
            query = query.Where(r => r.ProductId == productId.Value);
        if (warehouseId.HasValue)
            query = query.Where(r => r.WarehouseId == warehouseId.Value);
        return query;
    }
}
