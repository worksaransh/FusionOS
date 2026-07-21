using FusionOS.Modules.Core.Application.Branches.Contracts;
using FusionOS.Modules.Core.Domain.Organizations;
using FusionOS.Modules.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Core.Infrastructure.Repositories;

public sealed class BranchRepository : IBranchRepository
{
    private readonly CoreDbContext _context;

    public BranchRepository(CoreDbContext context) => _context = context;

    public Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default) =>
        _context.Branches.AnyAsync(b => b.CompanyId == companyId && b.Code == code.Trim().ToUpper(), cancellationToken);

    public Task<Branch?> GetByIdAsync(Guid companyId, Guid branchId, CancellationToken cancellationToken = default) =>
        _context.Branches.FirstOrDefaultAsync(b => b.CompanyId == companyId && b.Id == branchId, cancellationToken);

    public async Task AddAsync(Branch branch, CancellationToken cancellationToken = default) =>
        await _context.Branches.AddAsync(branch, cancellationToken);

    public async Task<IReadOnlyList<Branch>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderBy(b => b.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    private IQueryable<Branch> Filtered(Guid companyId, string? search)
    {
        var query = _context.Branches.Where(b => b.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(b => EF.Functions.ILike(b.Code, pattern) || EF.Functions.ILike(b.Name, pattern));
        }
        return query;
    }
}
