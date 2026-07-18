using FusionOS.Modules.Hrms.Application.LeaveRequests.Contracts;
using FusionOS.Modules.Hrms.Domain.LeaveRequests;
using FusionOS.Modules.Hrms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Hrms.Infrastructure.Repositories;

public sealed class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly HrmsDbContext _context;

    public LeaveRequestRepository(HrmsDbContext context) => _context = context;

    public Task<LeaveRequest?> GetByIdAsync(Guid companyId, Guid leaveRequestId, CancellationToken cancellationToken = default) =>
        _context.LeaveRequests.FirstOrDefaultAsync(r => r.CompanyId == companyId && r.Id == leaveRequestId, cancellationToken);

    public async Task AddAsync(LeaveRequest request, CancellationToken cancellationToken = default) =>
        await _context.LeaveRequests.AddAsync(request, cancellationToken);

    public async Task<IReadOnlyList<LeaveRequest>> ListAsync(Guid companyId, Guid? employeeId, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, employeeId)
            .OrderByDescending(r => r.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid? employeeId, CancellationToken cancellationToken = default) =>
        Filtered(companyId, employeeId).CountAsync(cancellationToken);

    private IQueryable<LeaveRequest> Filtered(Guid companyId, Guid? employeeId)
    {
        var query = _context.LeaveRequests.Where(r => r.CompanyId == companyId);
        if (employeeId.HasValue)
            query = query.Where(r => r.EmployeeId == employeeId.Value);
        return query;
    }
}
