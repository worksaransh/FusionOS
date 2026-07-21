using FusionOS.Modules.Inventory.Domain.Attributes;

namespace FusionOS.Modules.Inventory.Application.Attributes.Contracts;

public interface IAttributeDefinitionRepository
{
    Task<bool> NameExistsAsync(Guid companyId, string name, CancellationToken cancellationToken = default);
    Task<AttributeDefinition?> GetByIdAsync(Guid companyId, Guid attributeDefinitionId, CancellationToken cancellationToken = default);
    Task AddAsync(AttributeDefinition attributeDefinition, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttributeDefinition>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
}
