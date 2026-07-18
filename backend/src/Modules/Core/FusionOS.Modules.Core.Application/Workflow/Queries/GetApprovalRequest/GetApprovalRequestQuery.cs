using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Workflow.Contracts;

namespace FusionOS.Modules.Core.Application.Workflow.Queries.GetApprovalRequest;

public sealed record GetApprovalRequestQuery(Guid CompanyId, Guid Id) : IQuery<ApprovalRequestDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "core.approval-request.read" };
}
