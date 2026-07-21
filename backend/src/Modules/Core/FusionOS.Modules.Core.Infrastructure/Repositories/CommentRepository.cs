using FusionOS.Modules.Core.Application.Comments.Contracts;
using FusionOS.Modules.Core.Domain.Comments;
using FusionOS.Modules.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Core.Infrastructure.Repositories;

/// <summary>
/// Uses context.Set&lt;Comment&gt;() rather than a context.Comments DbSet
/// property — see CommentConfiguration's doc comment for why: the DbSet
/// property addition to CoreDbContext is a shared file another change is
/// touching this round, but EF Core doesn't require the property to include
/// the entity in the model.
/// </summary>
public sealed class CommentRepository : ICommentRepository
{
    private readonly CoreDbContext _context;

    public CommentRepository(CoreDbContext context) => _context = context;

    public async Task AddAsync(Comment comment, CancellationToken cancellationToken = default) =>
        await _context.Set<Comment>().AddAsync(comment, cancellationToken);

    public Task<Comment?> GetByIdAsync(Guid companyId, Guid id, CancellationToken cancellationToken = default) =>
        _context.Set<Comment>().FirstOrDefaultAsync(c => c.CompanyId == companyId && c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<CommentDto>> ListByEntityAsync(Guid companyId, string entityType, Guid entityId, CancellationToken cancellationToken = default) =>
        await _context.Set<Comment>()
            .Where(c => c.CompanyId == companyId && c.EntityType == entityType && c.EntityId == entityId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentDto(c.Id, c.EntityType, c.EntityId, c.Body, c.AuthorUserId, c.CreatedAt, c.UpdatedAt))
            .ToListAsync(cancellationToken);

    public Task RemoveAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        _context.Set<Comment>().Remove(comment);
        return Task.CompletedTask;
    }
}
