using FluentAssertions;
using FusionOS.Modules.Hrms.Domain.Payroll;
using FusionOS.Modules.Hrms.Domain.Payroll.Events;
using Xunit;

namespace FusionOS.Modules.Hrms.Tests.Payroll;

public class PayrollRecordTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid Employee = Guid.NewGuid();

    private static PayrollRecord New() =>
        PayrollRecord.CreateDraft(Company, Employee, 6, 2024, 5000m);

    [Fact]
    public void CreateDraft_WithValidFields_RaisesDraftedEventAndComputesTrivialGross()
    {
        var record = New();

        record.Status.Should().Be(PayrollStatus.Draft);
        record.BaseSalary.Should().Be(5000m);
        // Deliberately trivial — gross always equals base salary in this slice (see PayrollRecord.cs).
        record.GrossPay.Should().Be(record.BaseSalary);
        record.DomainEvents.Should().ContainSingle(e => e is PayrollRecordDrafted);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public void CreateDraft_WithInvalidPeriodMonth_Throws(int month)
    {
        var act = () => PayrollRecord.CreateDraft(Company, Employee, month, 2024, 5000m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateDraft_WithZeroBaseSalary_Throws()
    {
        var act = () => PayrollRecord.CreateDraft(Company, Employee, 6, 2024, 0m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreateDraft_WithEmptyEmployeeId_Throws()
    {
        var act = () => PayrollRecord.CreateDraft(Company, Guid.Empty, 6, 2024, 5000m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Approve_FromDraft_TransitionsAndSetsApprovedAt()
    {
        var record = New();
        var approvedAt = DateTimeOffset.UtcNow;

        record.Approve(approvedAt);

        record.Status.Should().Be(PayrollStatus.Approved);
        record.ApprovedAt.Should().Be(approvedAt);
    }

    [Fact]
    public void Approve_WhenNotDraft_Throws()
    {
        var record = New();
        record.Approve(DateTimeOffset.UtcNow);

        var act = () => record.Approve(DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkPaid_FromApproved_TransitionsAndSetsPaidAt()
    {
        var record = New();
        record.Approve(DateTimeOffset.UtcNow);
        var paidAt = DateTimeOffset.UtcNow;

        record.MarkPaid(paidAt);

        record.Status.Should().Be(PayrollStatus.Paid);
        record.PaidAt.Should().Be(paidAt);
    }

    [Fact]
    public void MarkPaid_WhenNotApproved_Throws()
    {
        var record = New();

        var act = () => record.MarkPaid(DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }
}
