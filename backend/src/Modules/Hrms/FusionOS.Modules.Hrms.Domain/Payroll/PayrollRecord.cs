using FusionOS.SharedKernel;
using FusionOS.Modules.Hrms.Domain.Payroll.Events;

namespace FusionOS.Modules.Hrms.Domain.Payroll;

/// <summary>
/// Phase 4 — HRMS: a deliberately minimal payroll skeleton, one record per
/// employee per calendar period (05_MODULE_ROADMAP.md's "Payroll" line item,
/// explicitly deferred by LeaveRequest's own class doc comment as a
/// separately-scoped follow-up — this is that follow-up, kept intentionally
/// small).
///
/// <b>This is NOT a payroll engine.</b> <see cref="GrossPay"/> is always
/// exactly <see cref="BaseSalary"/> — there are no allowances, no overtime,
/// no bonuses, and critically <b>no tax withholding or statutory deduction
/// calculation of any kind</b>. A real payroll engine needs jurisdiction-
/// specific tax slabs (which change every fiscal year), statutory
/// compliance rules, provident-fund/social-security contributions, and far
/// more — none of which this slice computes, approximates, or fakes.
/// Building a fake-but-complete-looking tax engine would be actively worse
/// than this honest, minimal skeleton, since it would look correct while
/// being wrong for every real company. What this slice does provide is a
/// real, working Draft -> Approved -> Paid record-keeping workflow so a
/// genuine calculation engine can be dropped in behind <see cref="GrossPay"/>
/// later without needing to change this aggregate's shape.
///
/// <see cref="EmployeeId"/> is a same-module reference validated by the
/// command handler, same convention as LeaveRequest.EmployeeId and
/// AttendanceRecord.EmployeeId. <see cref="BaseSalary"/> is a snapshot taken
/// at draft-creation time — Employee itself has no salary field in this
/// slice — same "snapshot, not a live cross-aggregate read" reasoning
/// Opportunity's CustomerName/ContactEmail use for their own Lead snapshot.
/// </summary>
public sealed class PayrollRecord : TenantAggregateRoot
{
    public Guid EmployeeId { get; private set; }
    public int PeriodMonth { get; private set; }
    public int PeriodYear { get; private set; }
    public decimal BaseSalary { get; private set; }

    /// <summary>Always equal to <see cref="BaseSalary"/> in this slice — see class doc comment for why nothing further is computed.</summary>
    public decimal GrossPay { get; private set; }

    public PayrollStatus Status { get; private set; }
    public DateTimeOffset? ApprovedAt { get; private set; }
    public DateTimeOffset? PaidAt { get; private set; }

    private PayrollRecord() { }

    public static PayrollRecord CreateDraft(Guid companyId, Guid employeeId, int periodMonth, int periodYear, decimal baseSalary)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee id is required.", nameof(employeeId));
        if (periodMonth is < 1 or > 12)
            throw new ArgumentException("Period month must be between 1 and 12.", nameof(periodMonth));
        if (periodYear < 2000)
            throw new ArgumentException("Period year is not valid.", nameof(periodYear));
        if (baseSalary <= 0)
            throw new ArgumentException("Base salary must be greater than zero.", nameof(baseSalary));

        var record = new PayrollRecord
        {
            CompanyId = companyId,
            EmployeeId = employeeId,
            PeriodMonth = periodMonth,
            PeriodYear = periodYear,
            BaseSalary = baseSalary,
            // Deliberately trivial: gross = base salary, no allowances/deductions/tax (see class doc comment).
            GrossPay = baseSalary,
            Status = PayrollStatus.Draft,
        };

        record.Raise(new PayrollRecordDrafted(record.Id, companyId, employeeId, periodMonth, periodYear));
        return record;
    }

    /// <summary>Draft -> Approved. Same "one clear starting state" discipline as LeaveRequest.Approve.</summary>
    public void Approve(DateTimeOffset approvedAt)
    {
        if (Status != PayrollStatus.Draft)
            throw new InvalidOperationException($"Only a Draft payroll record can be approved (current status: {Status}).");

        Status = PayrollStatus.Approved;
        ApprovedAt = approvedAt;
    }

    /// <summary>
    /// Approved -> Paid. Deliberately does not integrate with Finance/any bank
    /// payment rail or post a JournalEntry — it records the fact that this
    /// record was paid, nothing more, same restraint as FixedAsset.Dispose not
    /// calculating gain/loss. A real disbursement/GL-posting integration is a
    /// distinct, separately-scoped future slice.
    /// </summary>
    public void MarkPaid(DateTimeOffset paidAt)
    {
        if (Status != PayrollStatus.Approved)
            throw new InvalidOperationException($"Only an Approved payroll record can be marked paid (current status: {Status}).");

        Status = PayrollStatus.Paid;
        PaidAt = paidAt;
    }
}
