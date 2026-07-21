using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Comments.Contracts;

namespace FusionOS.Modules.Core.Application.Comments.Commands.CreateComment;

/// <summary>
/// Adds a comment to any (EntityType, EntityId) target — AuthorUserId is
/// always the authenticated caller (never a client-supplied value), same
/// reasoning as CreateApprovalRequestCommand's RequestedBy. EntityType/EntityId
/// double as the IAuditableCommand identity too, same as CreateApprovalRequestCommand.
/// </summary>
public sealed record CreateCommentCommand(Guid CompanyId, string EntityType, Guid EntityId, string Body)
    : ICommand<CommentDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.comment.create" };
    public string Action => "Commented";
}
