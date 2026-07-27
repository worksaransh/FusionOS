namespace FusionOS.Modules.Crm.Application.Tags.Contracts;

public interface IEntityTagRepository
{
    Task<Domain.Tags.EntityTag?> FindAsync(Guid companyId, Guid tagId, string entityType, Guid entityId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Tags.EntityTag entityTag, CancellationToken cancellationToken = default);
    void Remove(Domain.Tags.EntityTag entityTag);
    Task<IReadOnlyList<Domain.Tags.EntityTag>> ListForEntityAsync(Guid companyId, string entityType, Guid entityId, CancellationToken cancellationToken = default);
}
