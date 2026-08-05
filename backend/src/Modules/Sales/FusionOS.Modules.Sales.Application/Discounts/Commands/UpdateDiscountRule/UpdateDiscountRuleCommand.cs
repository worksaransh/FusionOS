using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Discounts.Contracts;

namespace FusionOS.Modules.Sales.Application.Discounts.Commands.UpdateDiscountRule;

public sealed record UpdateDiscountRuleCommand(Guid CompanyId, Guid DiscountRuleId, Guid ProductId, decimal MinQuantity, decimal DiscountPercentage)
    : ICommand<DiscountRuleDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.discount-rule.update" };
    public string EntityType => nameof(Domain.Discounts.DiscountRule);
    public Guid EntityId => DiscountRuleId;
    public string Action => "Updated";
}
