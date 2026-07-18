using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Domain.Employees;
using FusionOS.Modules.Hrms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Hrms.Infrastructure.Repositories;

public sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly HrmsDbContext _context;

    public EmployeeRepository(HrmsDbContext context) => _context = context;

    public Task<Employee?> GetByIdAsync(Guid companyId, Guid employeeId, CancellationToken cancellationToken = default) =>
        _context.Employees.FirstOrDefaultAsync(e => e.CompanyId == companyId && e.Id == employeeId, cancellationToken);

    public Task<bool> ExistsAsync(Guid companyId, Guid employeeId, CancellationToken cancellationToken = default) =>
        _context.Employees.AnyAsync(e => e.CompanyId == companyId && e.Id == employeeId, cancellationToken);

    public async Task AddAsync(Employee employee, CancellationToken cancellationToken = default) =>
        await _context.Employees.AddAsync(employee, cancellationToken);

    public async Task<IReadOnlyList<Employee>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderBy(e => e.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    private IQueryable<Employee> Filtered(Guid companyId, string? search)
    {
        var query = _context.Employees.Where(e => e.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(e => EF.Functions.ILike(e.Code, pattern) || EF.Functions.ILike(e.FullName, pattern));
        }
        return query;
    }
}
