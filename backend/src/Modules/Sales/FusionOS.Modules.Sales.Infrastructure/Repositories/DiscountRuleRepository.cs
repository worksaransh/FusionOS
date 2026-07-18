using FusionOS.Modules.Sales.Application.Discounts.Contracts;
using FusionOS.Modules.Sales.Domain.Discounts;
using FusionOS.Modules.Sales.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Sales.Infrastructure.Repositories;

public sealed class DiscountRuleRepository : IDiscountRuleRepository
{
    private readonly SalesDbContext _context;

    public DiscountRuleRepository(SalesDbContext context) => _context = context;

    public Task<DiscountRule?> GetByIdAsync(Guid companyId, Guid discountRuleId, CancellationToken cancellationToken = default) =>
        _context.DiscountRules.FirstOrDefaultAsync(r => r.CompanyId == companyId && r.Id == discountRuleId, cancellationToken);

    public async Task AddAsync(DiscountRule discountRule, CancellationToken cancellationToken = default) =>
        await _context.DiscountRules.AddAsync(discountRule, cancellationToken);

    public async Task<IReadOnlyList<DiscountRule>> ListAsync(Guid companyId, Guid? productId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, productId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid? productId, CancellationToken cancellationToken = default) =>
        Filtered(companyId, productId).CountAsync(cancellationToken);

    public async Task<IReadOnlyList<DiscountRule>> ListActiveForProductAsync(Guid companyId, Guid productId, CancellationToken cancellationToken = default) =>
        await _context.DiscountRules
            .Where(r => r.CompanyId == companyId && r.ProductId == productId && r.IsActive)
            .ToListAsync(cancellationToken);

    private IQueryable<DiscountRule> Filtered(Guid companyId, Guid? productId)
    {
        var query = _context.DiscountRules.Where(r => r.CompanyId == companyId);
        if (productId.HasValue)
            query = query.Where(r => r.ProductId == productId.Value);
        return query;
    }
}
