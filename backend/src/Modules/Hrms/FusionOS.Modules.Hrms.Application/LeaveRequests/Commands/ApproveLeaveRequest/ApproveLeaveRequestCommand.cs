using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Contracts;

namespace FusionOS.Modules.Hrms.Application.LeaveRequests.Commands.ApproveLeaveRequest;

public sealed record ApproveLeaveRequestCommand(Guid CompanyId, Guid LeaveRequestId)
    : ICommand<LeaveRequestDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "hrms.leave-request.approve" };
    public string EntityType => nameof(Domain.LeaveRequests.LeaveRequest);
    public Guid EntityId => LeaveRequestId;
    public string Action => "Approved";
}
