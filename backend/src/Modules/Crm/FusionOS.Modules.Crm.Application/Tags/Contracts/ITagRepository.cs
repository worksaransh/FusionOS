namespace FusionOS.Modules.Crm.Application.Tags.Contracts;

public interface ITagRepository
{
    Task<Domain.Tags.Tag?> GetByIdAsync(Guid companyId, Guid tagId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid companyId, Guid tagId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(Guid companyId, string name, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Tags.Tag tag, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Tags.Tag>> ListAsync(Guid companyId, string? search, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? search, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Tags.Tag>> GetByIdsAsync(Guid companyId, IReadOnlyCollection<Guid> tagIds, CancellationToken cancellationToken = default);
}
