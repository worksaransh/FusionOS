using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using FusionOS.Modules.Inventory.Domain.Attributes;
using FusionOS.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Inventory.Infrastructure.Repositories;

// NOTE: References InventoryDbContext.ProductVariantAttributeValues — see AttributeDefinitionRepository's note.
public sealed class ProductVariantAttributeValueRepository : IProductVariantAttributeValueRepository
{
    private readonly InventoryDbContext _context;

    public ProductVariantAttributeValueRepository(InventoryDbContext context) => _context = context;

    public Task<ProductVariantAttributeValue?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        _context.ProductVariantAttributeValues.FirstOrDefaultAsync(a => a.CompanyId == companyId && a.Id == id, cancellationToken);

    public Task<ProductVariantAttributeValue?> GetForVariantAndDefinitionAsync(Guid variantId, Guid attributeDefinitionId, CancellationToken cancellationToken = default) =>
        _context.ProductVariantAttributeValues.FirstOrDefaultAsync(a => a.VariantId == variantId && a.AttributeDefinitionId == attributeDefinitionId, cancellationToken);

    public async Task AddAsync(ProductVariantAttributeValue assignment, CancellationToken cancellationToken = default) =>
        await _context.ProductVariantAttributeValues.AddAsync(assignment, cancellationToken);

    public void Remove(ProductVariantAttributeValue assignment) => _context.ProductVariantAttributeValues.Remove(assignment);

    public async Task<IReadOnlyList<ProductVariantAttributeValue>> ListForVariantAsync(Guid variantId, CancellationToken cancellationToken = default) =>
        await _context.ProductVariantAttributeValues
            .Where(a => a.VariantId == variantId)
            .ToListAsync(cancellationToken);
}
