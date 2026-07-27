using FusionOS.SharedKernel;
using FusionOS.Modules.Hrms.Domain.LeaveRequests;

namespace FusionOS.Modules.Hrms.Domain.LeaveBalances;

/// <summary>
/// Phase 4 — HRMS: an employee's leave entitlement/accrual for one
/// <see cref="LeaveRequests.LeaveType"/> in one calendar/fiscal year
/// (05_MODULE_ROADMAP.md's "Leave" line item, closing the gap LeaveRequest's
/// own class doc comment left open — approval previously checked no balance
/// at all). EmployeeId is a same-module reference validated by the command
/// handler, same convention as LeaveRequest.EmployeeId/AttendanceRecord.EmployeeId.
///
/// One row per (CompanyId, EmployeeId, LeaveType, Year) — same "one row per
/// natural key" discipline AttendanceRecord uses for (CompanyId, EmployeeId,
/// Date): enforced here via a DB unique index (LeaveBalanceConfiguration)
/// plus a handler-level existence check (CreateLeaveBalanceCommandHandler),
/// same belt-and-suspenders split CreateLeaveRequestCommandHandler uses for
/// Employee existence.
///
/// <see cref="RemainingDays"/> is a computed property, not a persisted
/// column: it is always exactly EntitledDays - UsedDays, so persisting it
/// would only create a second place that value could drift out of sync with
/// its two inputs after a bug or manual DB edit — computing it keeps the
/// invariant structurally impossible to violate.
/// </summary>
public sealed class LeaveBalance : TenantAggregateRoot
{
    public Guid EmployeeId { get; private set; }
    public LeaveType LeaveType { get; private set; }
    public int Year { get; private set; }
    public decimal EntitledDays { get; private set; }
    public decimal UsedDays { get; private set; }

    /// <summary>Always EntitledDays - UsedDays — see class doc comment for why this isn't a persisted column.</summary>
    public decimal RemainingDays => EntitledDays - UsedDays;

    private LeaveBalance() { }

    public static LeaveBalance Create(Guid companyId, Guid employeeId, LeaveType leaveType, int year, decimal entitledDays)
    {
        if (employeeId == Guid.Empty)
            throw new ArgumentException("Employee id is required.", nameof(employeeId));
        if (year < 2000)
            throw new ArgumentException("Year is not valid.", nameof(year));
        if (entitledDays < 0)
            throw new ArgumentException("Entitled days cannot be negative.", nameof(entitledDays));

        return new LeaveBalance
        {
            CompanyId = companyId,
            EmployeeId = employeeId,
            LeaveType = leaveType,
            Year = year,
            EntitledDays = entitledDays,
            UsedDays = 0m,
        };
    }

    /// <summary>
    /// Records that <paramref name="days"/> more have been used against this balance —
    /// same "cap at the limit, throw if exceeded" idiom Manufacturing's
    /// WorkOrderComponent.Issue() uses for QuantityIssued/QuantityRequired. Cannot push
    /// UsedDays past EntitledDays.
    /// </summary>
    public void Deduct(decimal days)
    {
        if (days <= 0)
            throw new ArgumentException("Days to deduct must be greater than zero.", nameof(days));

        var newTotal = UsedDays + days;
        if (newTotal > EntitledDays)
        {
            throw new InvalidOperationException(
                $"Cannot deduct {days} day(s): only {EntitledDays - UsedDays} day(s) remain of this {Year} {LeaveType} balance.");
        }

        UsedDays = newTotal;
    }

    /// <summary>
    /// Reverses a prior deduction — the inverse of <see cref="Deduct"/>, same idiom as
    /// WorkOrderComponent.Return(), for when a previously-approved leave request is later
    /// rejected/cancelled/reversed after its days were already deducted. Cannot push
    /// UsedDays below zero.
    /// </summary>
    public void Restore(decimal days)
    {
        if (days <= 0)
            throw new ArgumentException("Days to restore must be greater than zero.", nameof(days));

        if (days > UsedDays)
        {
            throw new InvalidOperationException(
                $"Cannot restore {days} day(s): only {UsedDays} day(s) are currently used of this {Year} {LeaveType} balance.");
        }

        UsedDays -= days;
    }

    /// <summary>
    /// Manual correction/accrual top-up. Replaces EntitledDays outright rather than adding
    /// to it, so a caller correcting a mistake simply supplies the corrected total. Cannot
    /// be set below UsedDays — that would silently create a negative RemainingDays.
    /// </summary>
    public void AdjustEntitlement(decimal newEntitledDays)
    {
        if (newEntitledDays < 0)
            throw new ArgumentException("Entitled days cannot be negative.", nameof(newEntitledDays));
        if (newEntitledDays < UsedDays)
        {
            throw new InvalidOperationException(
                $"Cannot set entitlement to {newEntitledDays}: {UsedDays} day(s) are already used of this {Year} {LeaveType} balance.");
        }

        EntitledDays = newEntitledDays;
    }
}
