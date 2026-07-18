using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Products.Contracts;

namespace FusionOS.Modules.Inventory.Application.Products.Commands.DeactivateProductVariant;

public sealed record DeactivateProductVariantCommand(Guid CompanyId, Guid ProductId, Guid VariantId)
    : ICommand<ProductDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.product.update" };
    public string EntityType => nameof(Domain.Products.Product);
    public Guid EntityId => ProductId;
    public string Action => "VariantDeactivated";
}
