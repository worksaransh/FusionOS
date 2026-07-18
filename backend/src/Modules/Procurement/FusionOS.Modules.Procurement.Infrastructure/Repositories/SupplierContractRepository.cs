using FusionOS.Modules.Procurement.Application.SupplierContracts.Contracts;
using FusionOS.Modules.Procurement.Domain.SupplierContracts;
using FusionOS.Modules.Procurement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Procurement.Infrastructure.Repositories;

public sealed class SupplierContractRepository : ISupplierContractRepository
{
    private readonly ProcurementDbContext _context;

    public SupplierContractRepository(ProcurementDbContext context) => _context = context;

    public Task<SupplierContract?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        _context.SupplierContracts.FirstOrDefaultAsync(x => x.CompanyId == companyId && x.Id == id, cancellationToken);

    public async Task AddAsync(SupplierContract contract, CancellationToken cancellationToken = default) =>
        await _context.SupplierContracts.AddAsync(contract, cancellationToken);

    public async Task<IReadOnlyList<SupplierContract>> ListAsync(Guid companyId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.SupplierContracts
            .Where(x => x.CompanyId == companyId)
            .OrderByDescending(x => x.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, CancellationToken cancellationToken = default) =>
        _context.SupplierContracts.CountAsync(x => x.CompanyId == companyId, cancellationToken);
}
