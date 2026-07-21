using FusionOS.Modules.Core.Application.Departments.Contracts;
using FusionOS.Modules.Core.Domain.Organizations;
using FusionOS.Modules.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Core.Infrastructure.Repositories;

public sealed class DepartmentRepository : IDepartmentRepository
{
    private readonly CoreDbContext _context;

    public DepartmentRepository(CoreDbContext context) => _context = context;

    public Task<Department?> GetByIdAsync(Guid companyId, Guid departmentId, CancellationToken cancellationToken = default) =>
        _context.Departments.FirstOrDefaultAsync(d => d.CompanyId == companyId && d.Id == departmentId, cancellationToken);

    public async Task AddAsync(Department department, CancellationToken cancellationToken = default) =>
        await _context.Departments.AddAsync(department, cancellationToken);

    public async Task<IReadOnlyList<Department>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderBy(d => d.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    private IQueryable<Department> Filtered(Guid companyId, string? search)
    {
        var query = _context.Departments.Where(d => d.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(d => EF.Functions.ILike(d.Code, pattern) || EF.Functions.ILike(d.Name, pattern));
        }
        return query;
    }
}
