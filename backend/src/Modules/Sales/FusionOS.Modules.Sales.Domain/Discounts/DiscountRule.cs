using FusionOS.SharedKernel;
using FusionOS.Modules.Sales.Domain.Discounts.Events;

namespace FusionOS.Modules.Sales.Domain.Discounts;

/// <summary>
/// Phase 1 closeout (2026-07-18): one quantity-break tier of a Product's tiered
/// discount schedule — 05_MODULE_ROADMAP.md's Sales "pricing/discount rules
/// engine" line item's tiered half (PriceList already covers the per-customer
/// override-price half; a PriceList entry has no quantity dimension, which is
/// exactly what this fills). Multiple DiscountRule rows for the same
/// ProductId at different MinQuantity thresholds together form the "tiers" —
/// e.g. MinQuantity 10 -> 5%, MinQuantity 50 -> 10%, MinQuantity 100 -> 15%.
/// GetApplicableDiscountQuery (same module) picks the best matching tier for
/// a given Product+Quantity.
///
/// This is a lookup/suggestion the Sales Order creation flow queries, not an
/// automatic override of SalesOrderLine.DiscountPercentage — CreateSalesOrderCommand
/// already accepts a caller-supplied discount per line (validated against the
/// existing MaxDiscountPercentageWithoutApproval threshold), and silently
/// replacing whatever a salesperson negotiated with a tier lookup would be a
/// surprising, unrequested behavior change to that existing command. Same
/// "surface the number, let the human decide" restraint as Reservations'
/// available-to-promise query.
///
/// ProductId is an opaque reference into Inventory's Product aggregate, never
/// existence-validated here — same convention as SalesOrderLine's own ProductId.
/// </summary>
public sealed class DiscountRule : TenantAggregateRoot
{
    public Guid ProductId { get; private set; }
    public decimal MinQuantity { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public bool IsActive { get; private set; } = true;

    private DiscountRule() { }

    public static DiscountRule Create(Guid companyId, Guid productId, decimal minQuantity, decimal discountPercentage)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product id is required.", nameof(productId));
        if (minQuantity <= 0)
            throw new ArgumentException("Minimum quantity must be greater than zero.", nameof(minQuantity));
        if (discountPercentage <= 0 || discountPercentage > 100)
            throw new ArgumentException("Discount percentage must be greater than zero and no more than 100.", nameof(discountPercentage));

        var rule = new DiscountRule
        {
            CompanyId = companyId,
            ProductId = productId,
            MinQuantity = minQuantity,
            DiscountPercentage = discountPercentage,
        };

        rule.Raise(new DiscountRuleCreated(rule.Id, companyId, productId, minQuantity, discountPercentage));
        return rule;
    }

    public void Deactivate() => IsActive = false;
}
