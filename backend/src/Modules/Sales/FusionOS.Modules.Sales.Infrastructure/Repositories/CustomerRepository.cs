using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Sales.Infrastructure.Repositories;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly SalesDbContext _context;

    public CustomerRepository(SalesDbContext context) => _context = context;

    public Task<Domain.Customers.Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _context.Customers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> ExistsAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default) =>
        _context.Customers.AnyAsync(x => x.CompanyId == companyId && x.Id == customerId, cancellationToken);

    public Task<bool> CodeExistsAsync(Guid companyId, string code, CancellationToken cancellationToken = default) =>
        _context.Customers.AnyAsync(x => x.CompanyId == companyId && x.Code == code.Trim().ToUpper(), cancellationToken);

    public async Task AddAsync(Domain.Customers.Customer customer, CancellationToken cancellationToken = default) =>
        await _context.Customers.AddAsync(customer, cancellationToken);

    public async Task<IReadOnlyList<Domain.Customers.Customer>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, search)
            .OrderBy(x => x.Code)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default) =>
        Filtered(companyId, search).CountAsync(cancellationToken);

    private IQueryable<Domain.Customers.Customer> Filtered(Guid companyId, string? search)
    {
        var query = _context.Customers.Where(x => x.CompanyId == companyId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.Code, pattern) || EF.Functions.ILike(x.Name, pattern));
        }
        return query;
    }
}
