using FusionOS.Modules.Inventory.Domain.Attributes;

namespace FusionOS.Modules.Inventory.Application.Attributes.Contracts;

public interface IProductVariantAttributeValueRepository
{
    Task<ProductVariantAttributeValue?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>Used by the Assign handler's upsert check — see ProductVariantAttributeValue's doc comment ("one value per definition per variant").</summary>
    Task<ProductVariantAttributeValue?> GetForVariantAndDefinitionAsync(Guid variantId, Guid attributeDefinitionId, CancellationToken cancellationToken = default);
    Task AddAsync(ProductVariantAttributeValue assignment, CancellationToken cancellationToken = default);

    /// <summary>Real delete — see ProductVariantAttributeValue's doc comment for why this join/line-item record is hard-removed rather than soft-deactivated.</summary>
    void Remove(ProductVariantAttributeValue assignment);
    Task<IReadOnlyList<ProductVariantAttributeValue>> ListForVariantAsync(Guid variantId, CancellationToken cancellationToken = default);
}
