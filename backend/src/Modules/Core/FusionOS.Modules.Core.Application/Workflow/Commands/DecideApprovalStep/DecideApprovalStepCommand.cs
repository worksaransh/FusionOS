using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Workflow.Contracts;

namespace FusionOS.Modules.Core.Application.Workflow.Commands.DecideApprovalStep;

/// <summary>
/// Records a decision on whatever step is currently pending. The acting user
/// is always the authenticated caller (never client-supplied) — the domain
/// (ApprovalRequest.Decide) is what actually enforces that this user is the
/// step's assigned approver, same "data-dependent authorization" pattern as
/// Procurement's PO approve maker-checker check.
/// </summary>
public sealed record DecideApprovalStepCommand(Guid CompanyId, Guid ApprovalRequestId, bool Approve, string? Comments)
    : ICommand<ApprovalRequestDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "core.approval-request.decide" };
    public string EntityType => nameof(Domain.Workflow.ApprovalRequest);
    public Guid EntityId => ApprovalRequestId;
    public string Action => Approve ? "Approved" : "Rejected";
}
