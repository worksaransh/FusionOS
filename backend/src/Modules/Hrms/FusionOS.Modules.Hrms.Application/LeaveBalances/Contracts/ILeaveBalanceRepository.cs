using FusionOS.Modules.Hrms.Domain.LeaveRequests;

namespace FusionOS.Modules.Hrms.Application.LeaveBalances.Contracts;

public interface ILeaveBalanceRepository
{
    Task<Domain.LeaveBalances.LeaveBalance?> GetByIdAsync(Guid companyId, Guid leaveBalanceId, CancellationToken cancellationToken = default);

    /// <summary>Looks up the one balance row for (companyId, employeeId, leaveType, year) — the natural key ApproveLeaveRequestCommandHandler joins on.</summary>
    Task<Domain.LeaveBalances.LeaveBalance?> GetAsync(Guid companyId, Guid employeeId, LeaveType leaveType, int year, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid companyId, Guid employeeId, LeaveType leaveType, int year, CancellationToken cancellationToken = default);

    Task AddAsync(Domain.LeaveBalances.LeaveBalance balance, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Domain.LeaveBalances.LeaveBalance>> ListForEmployeeAsync(Guid companyId, Guid employeeId, CancellationToken cancellationToken = default);
}
