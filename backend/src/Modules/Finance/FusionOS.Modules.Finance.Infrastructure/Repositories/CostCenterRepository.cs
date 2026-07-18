using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using FusionOS.Modules.Finance.Domain.CostCenters;
using FusionOS.Modules.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Finance.Infrastructure.Repositories;

public sealed class CostCenterRepository : ICostCenterRepository
{
    private readonly FinanceDbContext _context;

    public CostCenterRepository(FinanceDbContext context) => _context = context;

    public Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default) =>
        _context.CostCenters.AnyAsync(c => c.CompanyId == companyId && c.Code == code.Trim().ToUpper(), cancellationToken);

    public Task<CostCenter?> GetByIdAsync(Guid companyId, Guid costCenterId, CancellationToken cancellationToken = default) =>
        _context.CostCenters.FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Id == costCenterId, cancellationToken);

    public async Task AddAsync(CostCenter costCenter, CancellationToken cancellationToken = default) =>
        await _context.CostCenters.AddAsync(costCenter, cancellationToken);

    public async Task<IReadOnlyList<CostCenter>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderBy(c => c.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    private IQueryable<CostCenter> Filtered(Guid companyId, string? search)
    {
        var query = _context.CostCenters.Where(c => c.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(c => EF.Functions.ILike(c.Code, pattern) || EF.Functions.ILike(c.Name, pattern));
        }
        return query;
    }
}
