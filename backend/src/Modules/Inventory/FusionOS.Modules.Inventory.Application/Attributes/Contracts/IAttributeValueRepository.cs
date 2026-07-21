using FusionOS.Modules.Inventory.Domain.Attributes;

namespace FusionOS.Modules.Inventory.Application.Attributes.Contracts;

public interface IAttributeValueRepository
{
    Task<bool> ValueExistsAsync(Guid attributeDefinitionId, string value, CancellationToken cancellationToken = default);
    Task<AttributeValue?> GetByIdAsync(Guid companyId, Guid attributeValueId, CancellationToken cancellationToken = default);
    Task AddAsync(AttributeValue attributeValue, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttributeValue>> ListByDefinitionAsync(Guid companyId, Guid attributeDefinitionId, CancellationToken cancellationToken = default);
}
