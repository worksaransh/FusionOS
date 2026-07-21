namespace FusionOS.Modules.Core.Application.Comments.Contracts;

public interface ICommentRepository
{
    Task AddAsync(Domain.Comments.Comment comment, CancellationToken cancellationToken = default);

    Task<Domain.Comments.Comment?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>Every comment on this (EntityType, EntityId) pair, oldest first — the natural reading order for a comment thread.</summary>
    Task<IReadOnlyList<CommentDto>> ListByEntityAsync(Guid companyId, string entityType, Guid entityId, CancellationToken cancellationToken = default);

    Task RemoveAsync(Domain.Comments.Comment comment, CancellationToken cancellationToken = default);
}
