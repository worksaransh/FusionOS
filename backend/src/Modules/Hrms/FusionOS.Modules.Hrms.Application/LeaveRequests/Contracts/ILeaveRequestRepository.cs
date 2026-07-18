namespace FusionOS.Modules.Hrms.Application.LeaveRequests.Contracts;

public interface ILeaveRequestRepository
{
    Task<Domain.LeaveRequests.LeaveRequest?> GetByIdAsync(Guid companyId, Guid leaveRequestId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.LeaveRequests.LeaveRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.LeaveRequests.LeaveRequest>> ListAsync(Guid companyId, Guid? employeeId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid? employeeId, CancellationToken cancellationToken = default);
}
