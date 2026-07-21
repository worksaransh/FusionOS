namespace FusionOS.Modules.Core.Application.Comments.Contracts;

public sealed record CommentDto(
    Guid Id,
    string EntityType,
    Guid EntityId,
    string Body,
    Guid AuthorUserId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
