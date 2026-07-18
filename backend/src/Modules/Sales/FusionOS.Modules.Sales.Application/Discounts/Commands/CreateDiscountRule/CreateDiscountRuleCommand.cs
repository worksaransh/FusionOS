using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Discounts.Contracts;

namespace FusionOS.Modules.Sales.Application.Discounts.Commands.CreateDiscountRule;

public sealed record CreateDiscountRuleCommand(Guid CompanyId, Guid ProductId, decimal MinQuantity, decimal DiscountPercentage)
    : ICommand<DiscountRuleDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.discount-rule.create" };
    public string EntityType => nameof(Domain.Discounts.DiscountRule);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
