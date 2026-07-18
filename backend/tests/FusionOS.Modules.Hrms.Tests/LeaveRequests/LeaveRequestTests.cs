using FluentAssertions;
using FusionOS.Modules.Hrms.Domain.LeaveRequests;
using FusionOS.Modules.Hrms.Domain.LeaveRequests.Events;
using Xunit;

namespace FusionOS.Modules.Hrms.Tests.LeaveRequests;

public class LeaveRequestTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid Employee = Guid.NewGuid();
    private static readonly DateTimeOffset Start = new(2024, 6, 1, 0, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset End = new(2024, 6, 5, 0, 0, 0, TimeSpan.Zero);

    private static LeaveRequest New() =>
        LeaveRequest.Create(Company, Employee, LeaveType.Annual, Start, End, "Family trip");

    [Fact]
    public void Create_Requested_RaisesCreatedEvent()
    {
        var request = New();

        request.Status.Should().Be(LeaveRequestStatus.Requested);
        request.DomainEvents.Should().ContainSingle(e => e is LeaveRequestCreated);
    }

    [Fact]
    public void Create_EndBeforeStart_Throws()
    {
        var act = () => LeaveRequest.Create(Company, Employee, LeaveType.Sick, End, Start, null);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Approve_FromRequested_TransitionsAndRaisesApproved()
    {
        var request = New();

        request.Approve();

        request.Status.Should().Be(LeaveRequestStatus.Approved);
        request.DomainEvents.Should().ContainSingle(e => e is LeaveRequestApproved);
    }

    [Fact]
    public void Approve_WhenNotRequested_Throws()
    {
        var request = New();
        request.Approve();

        var act = () => request.Approve();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reject_FromRequested_Transitions()
    {
        var request = New();

        request.Reject();

        request.Status.Should().Be(LeaveRequestStatus.Rejected);
    }

    [Fact]
    public void Reject_WhenNotRequested_Throws()
    {
        var request = New();
        request.Reject();

        var act = () => request.Reject();

        act.Should().Throw<InvalidOperationException>();
    }
}
