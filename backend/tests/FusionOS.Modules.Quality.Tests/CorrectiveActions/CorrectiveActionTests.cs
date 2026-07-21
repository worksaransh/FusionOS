using FluentAssertions;
using FusionOS.Modules.Quality.Domain.CorrectiveActions;
using FusionOS.Modules.Quality.Domain.CorrectiveActions.Events;
using Xunit;

namespace FusionOS.Modules.Quality.Tests.CorrectiveActions;

public class CorrectiveActionTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid NonConformanceReportId = Guid.NewGuid();
    private static readonly Guid AssignedTo = Guid.NewGuid();
    private static readonly DateTimeOffset DueDate = DateTimeOffset.UtcNow.AddDays(7);

    private static CorrectiveAction New() =>
        CorrectiveAction.Create(Company, NonConformanceReportId, "Worn tooling", "Replaced tooling", "Add tooling wear checklist", AssignedTo, DueDate);

    [Fact]
    public void Create_Open_RaisesCreatedEvent()
    {
        var capa = New();

        capa.Status.Should().Be(CorrectiveActionStatus.Open);
        capa.NonConformanceReportId.Should().Be(NonConformanceReportId);
        capa.DomainEvents.Should().ContainSingle(e => e is CorrectiveActionCreated);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_BlankRootCause_Throws(string rootCause)
    {
        var act = () => CorrectiveAction.Create(Company, NonConformanceReportId, rootCause, "fix", "prevent", AssignedTo, DueDate);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_EmptyNonConformanceReportId_Throws()
    {
        var act = () => CorrectiveAction.Create(Company, Guid.Empty, "root", "fix", "prevent", AssignedTo, DueDate);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_EmptyAssignedTo_Throws()
    {
        var act = () => CorrectiveAction.Create(Company, NonConformanceReportId, "root", "fix", "prevent", Guid.Empty, DueDate);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_DefaultDueDate_Throws()
    {
        var act = () => CorrectiveAction.Create(Company, NonConformanceReportId, "root", "fix", "prevent", AssignedTo, default);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Start_FromOpen_MovesToInProgress()
    {
        var capa = New();

        capa.Start();

        capa.Status.Should().Be(CorrectiveActionStatus.InProgress);
    }

    [Fact]
    public void Start_WhenNotOpen_Throws()
    {
        var capa = New();
        capa.Start();

        var act = () => capa.Start();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Close_FromInProgress_SetsClosedAt()
    {
        var capa = New();
        capa.Start();

        capa.Close();

        capa.Status.Should().Be(CorrectiveActionStatus.Closed);
        capa.ClosedAt.Should().NotBeNull();
    }

    [Fact]
    public void Close_WhenOpen_Throws()
    {
        var capa = New();

        var act = () => capa.Close();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Verify_FromClosed_SetsVerifiedAt_AndRaisesEvent()
    {
        var capa = New();
        capa.Start();
        capa.Close();

        capa.Verify();

        capa.Status.Should().Be(CorrectiveActionStatus.Verified);
        capa.VerifiedAt.Should().NotBeNull();
        capa.DomainEvents.Should().ContainSingle(e => e is CorrectiveActionVerified);
    }

    [Fact]
    public void Verify_WhenNotClosed_Throws()
    {
        var capa = New();
        capa.Start();

        var act = () => capa.Verify();

        act.Should().Throw<InvalidOperationException>();
    }
}
