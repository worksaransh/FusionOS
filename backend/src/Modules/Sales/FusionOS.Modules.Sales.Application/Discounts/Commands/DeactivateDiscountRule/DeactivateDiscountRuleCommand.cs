using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Discounts.Contracts;

namespace FusionOS.Modules.Sales.Application.Discounts.Commands.DeactivateDiscountRule;

public sealed record DeactivateDiscountRuleCommand(Guid CompanyId, Guid DiscountRuleId)
    : ICommand<DiscountRuleDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.discount-rule.deactivate" };
    public string EntityType => nameof(Domain.Discounts.DiscountRule);
    public Guid EntityId => DiscountRuleId;
    public string Action => "Deactivated";
}
