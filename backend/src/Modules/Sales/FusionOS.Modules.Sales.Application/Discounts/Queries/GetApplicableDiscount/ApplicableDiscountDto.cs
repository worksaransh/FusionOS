namespace FusionOS.Modules.Sales.Application.Discounts.Queries.GetApplicableDiscount;

/// <summary>DiscountRuleId is null and DiscountPercentage is 0 when no active tier's MinQuantity is met by the given quantity.</summary>
public sealed record ApplicableDiscountDto(Guid? DiscountRuleId, decimal DiscountPercentage);
