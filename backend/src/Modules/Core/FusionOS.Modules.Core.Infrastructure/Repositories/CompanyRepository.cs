using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Domain.Companies;
using FusionOS.Modules.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Core.Infrastructure.Repositories;

public sealed class CompanyRepository : ICompanyRepository
{
    private readonly CoreDbContext _context;

    public CompanyRepository(CoreDbContext context) => _context = context;

    public Task<Company?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Companies.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task AddAsync(Company company, CancellationToken cancellationToken = default) =>
        await _context.Companies.AddAsync(company, cancellationToken);

    public async Task<IReadOnlyList<Company>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default) =>
        await _context.Companies
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default) =>
        _context.Companies.CountAsync(cancellationToken);
}
