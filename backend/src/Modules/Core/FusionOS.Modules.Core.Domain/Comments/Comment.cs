using FusionOS.SharedKernel;

namespace FusionOS.Modules.Core.Domain.Comments;

/// <summary>
/// A user-authored note attached to any other entity in the system, identified
/// by the same (EntityType, EntityId) polymorphic-reference convention
/// FusionOS.Modules.Core.Domain.Workflow.ApprovalRequest established — this
/// engine has no opinion on what a "PurchaseOrder" or a "Lead" is, it only
/// tracks comments against some (EntityType, EntityId) pair, same as
/// ApprovalRequest tracks approvals against one.
///
/// Net-new (Phase — Comments/Activity Timeline): unlike AuditLog (system-
/// generated, insert-only, never updated per 04_DATABASE_GUIDELINES.md §5),
/// a Comment is user-authored and edit-in-place — its author can fix a typo
/// without leaving a second, "corrected" comment behind.
///
/// Deliberately NOT enforcing "only the author may edit" here in the domain
/// method itself. Every other "only the assigned/owning user may act"
/// rule already in this codebase (ApprovalRequest.Decide's assigned-approver
/// check, Notification's RecipientUserId check in
/// MarkNotificationReadCommandHandler) is a judgment call between "enforce in
/// the aggregate" and "enforce in the handler"; this one deliberately follows
/// the Notification precedent (handler-level check via ICurrentUserContext)
/// because the moderation override for Delete ("author OR a permission
/// holder") is unavoidably an app-layer/RBAC concern the domain has no access
/// to (Comment has no dependency on ICurrentUserContext or IAuthorizationService)
/// — putting the ownership check in the handler for both Update and Delete
/// keeps the "who may act" rule in one place instead of splitting it across
/// the aggregate (author check) and the handler (moderator check).
/// </summary>
public sealed class Comment : TenantAggregateRoot
{
    public const int MaxBodyLength = 4000;

    public string EntityType { get; private set; } = default!;
    public Guid EntityId { get; private set; }
    public string Body { get; private set; } = default!;
    public Guid AuthorUserId { get; private set; }

    private Comment() { }

    public static Comment Post(Guid companyId, string entityType, Guid entityId, Guid authorUserId, string body)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("Company id is required.", nameof(companyId));
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type is required.", nameof(entityType));
        if (entityId == Guid.Empty)
            throw new ArgumentException("Entity id is required.", nameof(entityId));
        if (authorUserId == Guid.Empty)
            throw new ArgumentException("Author user id is required.", nameof(authorUserId));

        var comment = new Comment
        {
            CompanyId = companyId,
            EntityType = entityType.Trim(),
            EntityId = entityId,
            AuthorUserId = authorUserId,
        };
        comment.SetBody(body);
        return comment;
    }

    /// <summary>
    /// Edits the comment's body in place. Author-only enforcement happens in
    /// UpdateCommentCommandHandler (see this class's doc comment) — this
    /// method only enforces the body's own shape invariants, same division of
    /// responsibility as Notification.MarkRead() leaving the "whose
    /// notification is this" check to its handler.
    /// </summary>
    public void UpdateBody(string newBody) => SetBody(newBody);

    private void SetBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Comment body is required.", nameof(body));
        if (body.Length > MaxBodyLength)
            throw new ArgumentException($"Comment body cannot exceed {MaxBodyLength} characters.", nameof(body));

        Body = body.Trim();
    }
}
