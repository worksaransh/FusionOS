using FusionOS.Modules.Warehouse.Application.Packages.Contracts;
using FusionOS.Modules.Warehouse.Domain.Packages;
using FusionOS.Modules.Warehouse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Warehouse.Infrastructure.Repositories;

public sealed class PackageRepository : IPackageRepository
{
    private readonly WarehouseDbContext _context;

    public PackageRepository(WarehouseDbContext context) => _context = context;

    // .Include(x => x.Lines) is required any time a parent whose child collection is backed by a
    // private field is queried — without it, Lines comes back empty (same note as PickListRepository).
    public Task<Package?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Packages.Include(p => p.Lines).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task AddAsync(Package package, CancellationToken cancellationToken = default) =>
        await _context.Packages.AddAsync(package, cancellationToken);

    public async Task<IReadOnlyList<Package>> ListByPickListAsync(Guid companyId, Guid pickListId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.Packages
            .Include(p => p.Lines)
            .Where(p => p.CompanyId == companyId && p.PickListId == pickListId)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountByPickListAsync(Guid companyId, Guid pickListId, CancellationToken cancellationToken = default) =>
        _context.Packages.CountAsync(p => p.CompanyId == companyId && p.PickListId == pickListId, cancellationToken);

    public Task<bool> PackageNumberExistsAsync(Guid companyId, Guid pickListId, string packageNumber, CancellationToken cancellationToken = default) =>
        _context.Packages.AnyAsync(p => p.CompanyId == companyId && p.PickListId == pickListId && p.PackageNumber == packageNumber, cancellationToken);

    // PackageLine is a plain FK-mapped entity (PackageConfiguration: HasMany(...).WithOne()
    // .HasForeignKey("PackageId")), not an EF owned type, so a SelectMany over the Lines navigation
    // translates to a single SQL join+SUM — same shape as DispatchRepository.GetDispatchedQuantityAsync
    // / InvoiceRepository.GetInvoicedQuantityAsync (Sales), not an in-memory sum.
    public Task<decimal> GetPackagedQuantityAsync(Guid companyId, Guid pickListId, Guid productId, CancellationToken cancellationToken = default)
    {
        var lines =
            from package in _context.Packages
            where package.CompanyId == companyId && package.PickListId == pickListId
            from line in package.Lines
            where line.ProductId == productId
            select line;

        return lines.SumAsync(l => l.Quantity, cancellationToken);
    }
}
