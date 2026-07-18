using FusionOS.BuildingBlocks.Application.Abstractions;

namespace FusionOS.Modules.Sales.Application.Discounts.Queries.GetApplicableDiscount;

public sealed record GetApplicableDiscountQuery(Guid CompanyId, Guid ProductId, decimal Quantity) : IQuery<ApplicableDiscountDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "sales.discount-rule.read" };
}
