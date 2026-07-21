using FusionOS.Modules.Hrms.Application.Attendance.Contracts;
using FusionOS.Modules.Hrms.Domain.Attendance;
using FusionOS.Modules.Hrms.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Hrms.Infrastructure.Repositories;

public sealed class AttendanceRecordRepository : IAttendanceRecordRepository
{
    private readonly HrmsDbContext _context;

    public AttendanceRecordRepository(HrmsDbContext context) => _context = context;

    public Task<AttendanceRecord?> GetByIdAsync(Guid companyId, Guid attendanceRecordId, CancellationToken cancellationToken = default) =>
        _context.AttendanceRecords.FirstOrDefaultAsync(a => a.CompanyId == companyId && a.Id == attendanceRecordId, cancellationToken);

    public async Task AddAsync(AttendanceRecord record, CancellationToken cancellationToken = default) =>
        await _context.AttendanceRecords.AddAsync(record, cancellationToken);

    public async Task<IReadOnlyList<AttendanceRecord>> ListAsync(Guid companyId, Guid? employeeId, DateTimeOffset? startDate, DateTimeOffset? endDate, int page, int pageSize, CancellationToken cancellationToken = default) =>
        await Filtered(companyId, employeeId, startDate, endDate)
            .OrderByDescending(a => a.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(Guid companyId, Guid? employeeId, DateTimeOffset? startDate, DateTimeOffset? endDate, CancellationToken cancellationToken = default) =>
        Filtered(companyId, employeeId, startDate, endDate).CountAsync(cancellationToken);

    private IQueryable<AttendanceRecord> Filtered(Guid companyId, Guid? employeeId, DateTimeOffset? startDate, DateTimeOffset? endDate)
    {
        var query = _context.AttendanceRecords.Where(a => a.CompanyId == companyId);
        if (employeeId.HasValue)
            query = query.Where(a => a.EmployeeId == employeeId.Value);
        if (startDate.HasValue)
            query = query.Where(a => a.Date >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(a => a.Date <= endDate.Value);
        return query;
    }
}
