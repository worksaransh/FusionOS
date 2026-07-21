namespace FusionOS.Modules.Hrms.Application.Attendance.Contracts;

public interface IAttendanceRecordRepository
{
    Task<Domain.Attendance.AttendanceRecord?> GetByIdAsync(Guid companyId, Guid attendanceRecordId, CancellationToken cancellationToken = default);
    Task AddAsync(Domain.Attendance.AttendanceRecord record, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Attendance.AttendanceRecord>> ListAsync(Guid companyId, Guid? employeeId, DateTimeOffset? startDate, DateTimeOffset? endDate, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Guid companyId, Guid? employeeId, DateTimeOffset? startDate, DateTimeOffset? endDate, CancellationToken cancellationToken = default);
}
