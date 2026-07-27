using FusionOS.SharedKernel;

namespace FusionOS.Modules.Hrms.Domain.Payroll.Events;

/// <summary>
/// Raised once a payroll record is marked paid (<see cref="PayrollRecord.MarkPaid"/>).
/// Unlike <see cref="PayrollRecordDrafted"/>, this one does have a consumer:
/// Finance's PayrollRecordPaidConsumer reacts to it to post a Debit Salary
/// Expense / Credit Salary Payable JournalEntry, closing the gap
/// <see cref="PayrollRecord.MarkPaid"/>'s own doc comment flagged as
/// "a distinct, separately-scoped future slice." Carries <see cref="GrossPay"/>
/// (this slice's only pay figure — see PayrollRecord's class doc comment for
/// why nothing further is computed) and the pay period identifiers so a
/// consumer can build a human-readable GL reference without a cross-module
/// callback into HRMS.
/// </summary>
public sealed record PayrollRecordPaid(
    Guid PayrollRecordId,
    Guid CompanyId,
    Guid EmployeeId,
    decimal GrossPay,
    int PeriodMonth,
    int PeriodYear) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
