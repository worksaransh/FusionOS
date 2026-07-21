using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Comments.Contracts;

namespace FusionOS.Modules.Core.Application.Comments.Commands.UpdateComment;

/// <summary>
/// Edits an existing comment's body in place. Gated by "core.comment.create"
/// rather than a dedicated "update" code — creating and editing your own
/// comment are the same trust level (both just mean "you're allowed to write
/// comments"), so a separate permission code would only add catalog noise
/// without a real distinction. Author-only ownership is enforced in the
/// handler (see Comment's doc comment for why), not by this permission gate —
/// holding "core.comment.create" is necessary but not sufficient; you must
/// also be this comment's author.
/// </summary>
public sealed record UpdateCommentCommand(Guid CompanyId, Guid CommentId, string Body)
    : ICommand<CommentDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.comment.create" };
    public string EntityType => nameof(Domain.Comments.Comment);
    public Guid EntityId => CommentId;
    public string Action => "Updated";
}
