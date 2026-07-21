namespace FusionOS.Modules.Crm.Application.Activities.Contracts;

public interface IActivityRepository
{
    Task<Domain.Activities.Activity?> GetByIdAsync(Guid companyId, Guid activityId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Activities.Activity activity, CancellationToken cancellationToken = default);

    /// <summary>Both filters are optional — omit both for a company-wide feed, or supply both to see one record's history (the (EntityType, EntityId) pair only makes sense together).</summary>
    Task<IReadOnlyList<Domain.Activities.Activity>> ListAsync(Guid companyId, string? entityType, Guid? entityId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, string? entityType, Guid? entityId, CancellationToken cancellationToken = default);
}
