using FusionOS.Modules.Inventory.Application.Attributes.Contracts;
using FusionOS.Modules.Inventory.Domain.Attributes;
using FusionOS.Modules.Inventory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Inventory.Infrastructure.Repositories;

// NOTE: References InventoryDbContext.AttributeValues — see AttributeDefinitionRepository's note.
public sealed class AttributeValueRepository : IAttributeValueRepository
{
    private readonly InventoryDbContext _context;

    public AttributeValueRepository(InventoryDbContext context) => _context = context;

    public Task<bool> ValueExistsAsync(Guid attributeDefinitionId, string value, CancellationToken cancellationToken = default) =>
        _context.AttributeValues.AnyAsync(v => v.AttributeDefinitionId == attributeDefinitionId && EF.Functions.ILike(v.Value, value.Trim()), cancellationToken);

    public Task<AttributeValue?> GetByIdAsync(Guid companyId, Guid attributeValueId, CancellationToken cancellationToken = default) =>
        _context.AttributeValues.FirstOrDefaultAsync(v => v.CompanyId == companyId && v.Id == attributeValueId, cancellationToken);

    public async Task AddAsync(AttributeValue attributeValue, CancellationToken cancellationToken = default) =>
        await _context.AttributeValues.AddAsync(attributeValue, cancellationToken);

    public async Task<IReadOnlyList<AttributeValue>> ListByDefinitionAsync(Guid companyId, Guid attributeDefinitionId, CancellationToken cancellationToken = default) =>
        await _context.AttributeValues
            .Where(v => v.CompanyId == companyId && v.AttributeDefinitionId == attributeDefinitionId)
            .OrderBy(v => v.Value)
            .ToListAsync(cancellationToken);
}
