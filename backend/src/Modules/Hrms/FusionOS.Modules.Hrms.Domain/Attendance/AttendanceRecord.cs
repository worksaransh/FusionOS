using FusionOS.SharedKernel;
using FusionOS.Modules.Hrms.Domain.Attendance.Events;

namespace FusionOS.Modules.Hrms.Domain.Attendance;

/// <summary>
/// Phase 4 — HRMS: an employee's attendance for a single calendar date
/// (05_MODULE_ROADMAP.md's "Attendance" line item, explicitly deferred by
/// LeaveRequest's own class doc comment as a separately-scoped follow-up —
/// this is that follow-up).
///
/// <see cref="EmployeeId"/> is a real, same-module foreign key (Employee
/// lives in this module), validated by the command handler via
/// IEmployeeRepository — same convention CreateLeaveRequestCommandHandler
/// already uses for its own EmployeeId.
///
/// <see cref="LeaveRequestId"/> is an optional same-module reference: when an
/// Absent/HalfDay/OnLeave day is explained by an already-approved leave
/// request, the record can point at it. There is no FK constraint — existence
/// is validated in the command handler when supplied, same "reference-by-id,
/// no database FK, existence validated in the handler" convention Opportunity
/// uses for its LeadId (see Opportunity.cs's own doc comment) and
/// LeaveRequest uses for its EmployeeId.
///
/// Payroll/Recruitment/Performance/Training remain out of scope for this
/// slice.
/// </summary>
public sealed class AttendanceRecord : TenantAggregateRoot
{
    public Guid EmployeeId { get; private set; }
    public DateTimeOffset Date { get; private set; }
    public DateTimeOffset? CheckInTime { get; private set; }
    public DateTimeOffset? CheckOutTime { get; private set; }
    public AttendanceStatus Status { get; private set; }
    public Guid? LeaveRequestId { get; private set; }

    private AttendanceRecord() { }

    public static AttendanceRecord Create(
        Guid companyId,
        Guid employeeId,
        DateTimeOffset date,
        DateTimeOffset? checkInTime,
        DateTimeOffset? checkOutTime,
        AttendanceStatus status,
        Guid? leaveRequestId)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee id is required.", nameof(employeeId));
        if (checkInTime.HasValue && checkOutTime.HasValue && checkOutTime.Value < checkInTime.Value)
            throw new ArgumentException("Check-out time cannot be before check-in time.", nameof(checkOutTime));

        var record = new AttendanceRecord
        {
            CompanyId = companyId,
            EmployeeId = employeeId,
            Date = date,
            CheckInTime = checkInTime,
            CheckOutTime = checkOutTime,
            Status = status,
            LeaveRequestId = leaveRequestId,
        };

        record.Raise(new AttendanceRecorded(record.Id, companyId, employeeId, status.ToString()));
        return record;
    }

    /// <summary>
    /// Corrects an already-recorded day (e.g. a forgotten check-out time entered
    /// later, or a status correction). There is no workflow state machine here
    /// — unlike LeaveRequest.Approve/Reject, this is a plain master-data
    /// correction, not a one-way business transition, so it has no "one clear
    /// starting state" guard.
    /// </summary>
    public void Update(DateTimeOffset? checkInTime, DateTimeOffset? checkOutTime, AttendanceStatus status, Guid? leaveRequestId)
    {
        if (checkInTime.HasValue && checkOutTime.HasValue && checkOutTime.Value < checkInTime.Value)
            throw new ArgumentException("Check-out time cannot be before check-in time.", nameof(checkOutTime));

        CheckInTime = checkInTime;
        CheckOutTime = checkOutTime;
        Status = status;
        LeaveRequestId = leaveRequestId;
    }
}
