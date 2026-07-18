using FusionOS.SharedKernel;
using FusionOS.Modules.Hrms.Domain.LeaveRequests.Events;

namespace FusionOS.Modules.Hrms.Domain.LeaveRequests;

/// <summary>
/// Phase 4 — HRMS, first slice: an employee's leave request
/// (05_MODULE_ROADMAP.md's "Leave" line item), Requested → Approved/Rejected.
/// EmployeeId is a real, same-module foreign key (Employee lives in this
/// module), validated by the command handler via IEmployeeRepository — same
/// convention as CreateBudgetLine validating AccountId in Finance.
/// Attendance/Payroll/Recruitment/Performance/Training are explicitly out of
/// scope for this slice — separately scoped follow-ups.
/// </summary>
public sealed class LeaveRequest : TenantAggregateRoot
{
    public Guid EmployeeId { get; private set; }
    public LeaveType Type { get; private set; }
    public DateTimeOffset StartDate { get; private set; }
    public DateTimeOffset EndDate { get; private set; }
    public string? Reason { get; private set; }
    public LeaveRequestStatus Status { get; private set; }

    private LeaveRequest() { }

    public static LeaveRequest Create(Guid companyId, Guid employeeId, LeaveType type, DateTimeOffset startDate, DateTimeOffset endDate, string? reason)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee id is required.", nameof(employeeId));
        if (endDate < startDate)
            throw new ArgumentException("End date cannot be before start date.", nameof(endDate));

        var request = new LeaveRequest
        {
            CompanyId = companyId,
            EmployeeId = employeeId,
            Type = type,
            StartDate = startDate,
            EndDate = endDate,
            Reason = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim(),
            Status = LeaveRequestStatus.Requested,
        };

        request.Raise(new LeaveRequestCreated(request.Id, companyId, employeeId, type.ToString()));
        return request;
    }

    /// <summary>Requires the request to still be Requested — same "one clear starting state" discipline as MaintenanceRequest.Start.</summary>
    public void Approve()
    {
        if (Status != LeaveRequestStatus.Requested)
            throw new InvalidOperationException($"Only a Requested leave request can be approved (current status: {Status}).");

        Status = LeaveRequestStatus.Approved;
        Raise(new LeaveRequestApproved(Id, CompanyId, EmployeeId, StartDate, EndDate));
    }

    public void Reject()
    {
        if (Status != LeaveRequestStatus.Requested)
            throw new InvalidOperationException($"Only a Requested leave request can be rejected (current status: {Status}).");

        Status = LeaveRequestStatus.Rejected;
    }
}
