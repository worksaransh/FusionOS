using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Products.Contracts;

namespace FusionOS.Modules.Inventory.Application.Products.Commands.RemoveUnitOfMeasureConversion;

/// <summary>M9 remaining — Multi-UOM (2026-07-16). Modeled as a POST action, not a DELETE, per this codebase's "apiClient has no delete method by design" convention (see ProductsPage.tsx doc comment) — the same reasoning as every other soft/POST-only mutation here.</summary>
public sealed record RemoveUnitOfMeasureConversionCommand(Guid CompanyId, Guid ProductId, string AlternateUnitOfMeasure)
    : ICommand<ProductDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "inventory.product.update" };
    public string EntityType => nameof(Domain.Products.Product);
    public Guid EntityId => ProductId;
    public string Action => "UnitOfMeasureConversionRemoved";
}
