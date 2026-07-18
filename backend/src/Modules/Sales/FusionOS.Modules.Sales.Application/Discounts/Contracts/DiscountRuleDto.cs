namespace FusionOS.Modules.Sales.Application.Discounts.Contracts;

public sealed record DiscountRuleDto(Guid Id, Guid ProductId, decimal MinQuantity, decimal DiscountPercentage, bool IsActive);

/// <summary>Single place that turns a DiscountRule aggregate into its DTO, shared by every handler that returns one.</summary>
public static class DiscountRuleMapper
{
    public static DiscountRuleDto ToDto(Domain.Discounts.DiscountRule rule) => new(
        rule.Id, rule.ProductId, rule.MinQuantity, rule.DiscountPercentage, rule.IsActive);
}
