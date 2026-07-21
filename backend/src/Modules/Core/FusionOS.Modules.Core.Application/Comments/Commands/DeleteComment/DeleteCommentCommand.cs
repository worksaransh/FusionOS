using FusionOS.BuildingBlocks.Application.Abstractions;

namespace FusionOS.Modules.Core.Application.Comments.Commands.DeleteComment;

/// <summary>
/// Deletes a comment — allowed for its own author OR anyone holding
/// "core.comment.delete" (a moderation override). Deliberately does NOT
/// implement IRequirePermission: AuthorizationBehavior's blanket gate is an
/// AND of every RequiredPermissions entry, and there is no single permission
/// code that correctly expresses "the author OR a permission holder" — an
/// author without "core.comment.delete" must still be able to delete their
/// own comment. So this command is intentionally unauthorized at the pipeline
/// level, and DeleteCommentCommandHandler performs the full
/// "am I the author, or do I hold core.comment.delete" check itself via
/// ICurrentUserContext (same escape hatch AuthorizationBehavior itself is
/// built on: ICurrentUserContext.HasPermission(code)).
/// </summary>
public sealed record DeleteCommentCommand(Guid CompanyId, Guid CommentId) : ICommand, IAuditableCommand
{
    public string EntityType => nameof(Domain.Comments.Comment);
    public Guid EntityId => CommentId;
    public string Action => "Deleted";
}
