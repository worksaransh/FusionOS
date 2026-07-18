using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Contracts;
using FusionOS.Modules.Hrms.Domain.LeaveRequests;

namespace FusionOS.Modules.Hrms.Application.LeaveRequests.Commands.CreateLeaveRequest;

public sealed record CreateLeaveRequestCommand(Guid CompanyId, Guid EmployeeId, LeaveType Type, DateTimeOffset StartDate, DateTimeOffset EndDate, string? Reason)
    : ICommand<LeaveRequestDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "hrms.leave-request.create" };
    public string EntityType => nameof(Domain.LeaveRequests.LeaveRequest);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
