using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Workflow.Contracts;

namespace FusionOS.Modules.Core.Application.Workflow.Queries.ListPendingApprovals;

/// <summary>"My pending approvals" — always scoped to the calling user via ICurrentUserContext, never a client-supplied user id, so one user can never list another's pending approvals.</summary>
public sealed record ListPendingApprovalsQuery(Guid CompanyId, int Page, int PageSize) : IQuery<PagedResult<ApprovalRequestDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.approval-request.read" };
}
