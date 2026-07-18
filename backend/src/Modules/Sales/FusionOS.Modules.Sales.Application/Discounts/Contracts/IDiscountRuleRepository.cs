namespace FusionOS.Modules.Sales.Application.Discounts.Contracts;

public interface IDiscountRuleRepository
{
    Task<Domain.Discounts.DiscountRule?> GetByIdAsync(Guid companyId, Guid discountRuleId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Discounts.DiscountRule discountRule, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Discounts.DiscountRule>> ListAsync(Guid companyId, Guid? productId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid? productId, CancellationToken cancellationToken = default);

    /// <summary>Every active tier for a Product, used by GetApplicableDiscountQuery to pick the best-matching one for a given quantity.</summary>
    Task<IReadOnlyList<Domain.Discounts.DiscountRule>> ListActiveForProductAsync(Guid companyId, Guid productId, CancellationToken cancellationToken = default);
}
