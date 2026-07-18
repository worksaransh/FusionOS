using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Contracts;

namespace FusionOS.Modules.Hrms.Application.LeaveRequests.Commands.RejectLeaveRequest;

public sealed record RejectLeaveRequestCommand(Guid CompanyId, Guid LeaveRequestId)
    : ICommand<LeaveRequestDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "hrms.leave-request.reject" };
    public string EntityType => nameof(Domain.LeaveRequests.LeaveRequest);
    public Guid EntityId => LeaveRequestId;
    public string Action => "Rejected";
}
