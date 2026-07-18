using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Workflow.Contracts;

namespace FusionOS.Modules.Core.Application.Workflow.Commands.CreateApprovalRequest;

/// <summary>
/// Submits a new approval chain for some (EntityType, EntityId) pair the
/// caller owns — RequestedBy is always the authenticated caller (never a
/// client-supplied value), same reasoning as every other command in this
/// codebase that records "who did this." EntityType/EntityId double as the
/// IAuditableCommand identity too — the audit trail for "an approval was
/// requested" is naturally about the same target entity the approval itself
/// is for.
/// </summary>
public sealed record CreateApprovalRequestCommand(Guid CompanyId, string EntityType, Guid EntityId, IReadOnlyList<Guid> ApproverUserIds)
    : ICommand<ApprovalRequestDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.approval-request.create" };
    public string Action => "Submitted";
}
