using FluentAssertions;
using FusionOS.Modules.Maintenance.Domain.MaintenanceRequests;
using FusionOS.Modules.Maintenance.Domain.MaintenanceRequests.Events;
using Xunit;

namespace FusionOS.Modules.Maintenance.Tests.MaintenanceRequests;

public class MaintenanceRequestTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid Asset = Guid.NewGuid();

    private static MaintenanceRequest New() =>
        MaintenanceRequest.Create(Company, Asset, MaintenanceRequestType.Breakdown, "Motor overheating");

    [Fact]
    public void Create_Open_RaisesCreatedEvent()
    {
        var request = New();

        request.Status.Should().Be(MaintenanceRequestStatus.Open);
        request.DomainEvents.Should().ContainSingle(e => e is MaintenanceRequestCreated);
    }

    [Fact]
    public void Create_WithBlankDescription_Throws()
    {
        var act = () => MaintenanceRequest.Create(Company, Asset, MaintenanceRequestType.Preventive, "  ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Start_FromOpen_TransitionsToInProgress()
    {
        var request = New();

        request.Start();

        request.Status.Should().Be(MaintenanceRequestStatus.InProgress);
    }

    [Fact]
    public void Start_WhenNotOpen_Throws()
    {
        var request = New();
        request.Start();

        var act = () => request.Start();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Complete_FromInProgress_ResolvesAndRaisesCompleted()
    {
        var request = New();
        request.Start();

        request.Complete("Replaced bearing");

        request.Status.Should().Be(MaintenanceRequestStatus.Completed);
        request.ResolutionNotes.Should().Be("Replaced bearing");
        request.CompletedAt.Should().NotBeNull();
        request.DomainEvents.Should().ContainSingle(e => e is MaintenanceRequestCompleted);
    }

    [Fact]
    public void Complete_WhenNotInProgress_Throws()
    {
        var request = New();

        var act = () => request.Complete(null);

        act.Should().Throw<InvalidOperationException>();
    }
}
