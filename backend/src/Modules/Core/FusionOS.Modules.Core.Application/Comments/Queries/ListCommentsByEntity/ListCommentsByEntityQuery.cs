using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Comments.Contracts;

namespace FusionOS.Modules.Core.Application.Comments.Queries.ListCommentsByEntity;

/// <summary>Every comment on a given (EntityType, EntityId), oldest first.</summary>
public sealed record ListCommentsByEntityQuery(Guid CompanyId, string EntityType, Guid EntityId)
    : IQuery<IReadOnlyList<CommentDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.comment.read" };
}
