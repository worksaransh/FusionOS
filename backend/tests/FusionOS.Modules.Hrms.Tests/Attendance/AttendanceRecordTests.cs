using FluentAssertions;
using FusionOS.Modules.Hrms.Domain.Attendance;
using FusionOS.Modules.Hrms.Domain.Attendance.Events;
using Xunit;

namespace FusionOS.Modules.Hrms.Tests.Attendance;

public class AttendanceRecordTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid Employee = Guid.NewGuid();
    private static readonly DateTimeOffset Date = new(2024, 6, 3, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset CheckIn = new(2024, 6, 3, 9, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset CheckOut = new(2024, 6, 3, 17, 0, 0, TimeSpan.Zero);

    private static AttendanceRecord New() =>
        AttendanceRecord.Create(Company, Employee, Date, CheckIn, CheckOut, AttendanceStatus.Present, null);

    [Fact]
    public void Create_WithValidFields_RaisesRecordedEvent()
    {
        var record = New();

        record.EmployeeId.Should().Be(Employee);
        record.Status.Should().Be(AttendanceStatus.Present);
        record.CheckInTime.Should().Be(CheckIn);
        record.CheckOutTime.Should().Be(CheckOut);
        record.LeaveRequestId.Should().BeNull();
        record.DomainEvents.Should().ContainSingle(e => e is AttendanceRecorded);
    }

    [Fact]
    public void Create_WithEmptyEmployeeId_Throws()
    {
        var act = () => AttendanceRecord.Create(Company, Guid.Empty, Date, CheckIn, CheckOut, AttendanceStatus.Present, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_CheckOutBeforeCheckIn_Throws()
    {
        var act = () => AttendanceRecord.Create(Company, Employee, Date, CheckOut, CheckIn, AttendanceStatus.Present, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_OnLeaveWithLeaveRequestId_SetsLeaveRequestId()
    {
        var leaveRequestId = Guid.NewGuid();

        var record = AttendanceRecord.Create(Company, Employee, Date, null, null, AttendanceStatus.OnLeave, leaveRequestId);

        record.Status.Should().Be(AttendanceStatus.OnLeave);
        record.LeaveRequestId.Should().Be(leaveRequestId);
    }

    [Fact]
    public void Update_CorrectsFields()
    {
        var record = AttendanceRecord.Create(Company, Employee, Date, null, null, AttendanceStatus.Absent, null);

        record.Update(CheckIn, CheckOut, AttendanceStatus.Present, null);

        record.Status.Should().Be(AttendanceStatus.Present);
        record.CheckInTime.Should().Be(CheckIn);
        record.CheckOutTime.Should().Be(CheckOut);
    }

    [Fact]
    public void Update_CheckOutBeforeCheckIn_Throws()
    {
        var record = New();

        var act = () => record.Update(CheckOut, CheckIn, AttendanceStatus.Present, null);

        act.Should().Throw<ArgumentException>();
    }
}
