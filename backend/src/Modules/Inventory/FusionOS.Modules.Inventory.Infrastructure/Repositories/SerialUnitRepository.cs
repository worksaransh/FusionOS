using FusionOS.Modules.Inventory.Application.SerialUnits.Contracts;
using FusionOS.Modules.Inventory.Domain.SerialUnits;
using FusionOS.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Inventory.Infrastructure.Repositories;

/// <summary>
/// Uses DbContext.Set&lt;SerialUnit&gt;() rather than a dedicated
/// InventoryDbContext.SerialUnits DbSet property — see BatchRepository's doc
/// comment for why (the entity is still fully registered in the model via
/// SerialUnitConfiguration; this just avoids touching InventoryDbContext.cs).
/// </summary>
public sealed class SerialUnitRepository : ISerialUnitRepository
{
    private readonly InventoryDbContext _context;

    public SerialUnitRepository(InventoryDbContext context) => _context = context;

    private DbSet<SerialUnit> SerialUnits => _context.Set<SerialUnit>();

    public Task<SerialUnit?> GetByIdAsync(Guid companyId, Guid serialUnitId, CancellationToken cancellationToken = default) =>
        SerialUnits.FirstOrDefaultAsync(s => s.CompanyId == companyId && s.Id == serialUnitId, cancellationToken);

    public Task<SerialUnit?> GetBySerialNumberAsync(Guid companyId, string serialNumber, CancellationToken cancellationToken = default) =>
        SerialUnits.FirstOrDefaultAsync(s => s.CompanyId == companyId && s.SerialNumber == serialNumber.Trim(), cancellationToken);

    public Task<bool> SerialNumberExistsAsync(Guid companyId, Guid productId, string serialNumber, CancellationToken cancellationToken = default) =>
        SerialUnits.AnyAsync(s => s.CompanyId == companyId && s.ProductId == productId && s.SerialNumber == serialNumber.Trim(), cancellationToken);

    public async Task AddAsync(SerialUnit unit, CancellationToken cancellationToken = default) =>
        await SerialUnits.AddAsync(unit, cancellationToken);

    public async Task<IReadOnlyList<SerialUnit>> ListByProductAsync(Guid companyId, Guid productId, SerialUnitStatus? status, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, productId, status)
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountByProductAsync(Guid companyId, Guid productId, SerialUnitStatus? status, CancellationToken cancellationToken = default) =>
        Filtered(companyId, productId, status).CountAsync(cancellationToken);

    private IQueryable<SerialUnit> Filtered(Guid companyId, Guid productId, SerialUnitStatus? status)
    {
        var query = SerialUnits.Where(s => s.CompanyId == companyId && s.ProductId == productId);
        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);
        return query;
    }
}
