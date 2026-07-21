using FluentAssertions;
using FusionOS.Modules.Maintenance.Domain.MaintenanceSchedules;
using FusionOS.Modules.Maintenance.Domain.MaintenanceSchedules.Events;
using Xunit;

namespace FusionOS.Modules.Maintenance.Tests.MaintenanceSchedules;

public class MaintenanceScheduleTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid Asset = Guid.NewGuid();
    private static readonly DateTimeOffset NextDueDate = DateTimeOffset.UtcNow.AddDays(30);

    private static MaintenanceSchedule New() =>
        MaintenanceSchedule.Create(Company, Asset, MaintenanceScheduleFrequency.Quarterly, "Oil change", NextDueDate);

    [Fact]
    public void Create_Active_RaisesCreatedEvent()
    {
        var schedule = New();

        schedule.IsActive.Should().BeTrue();
        schedule.Frequency.Should().Be(MaintenanceScheduleFrequency.Quarterly);
        schedule.NextDueDate.Should().Be(NextDueDate);
        schedule.DomainEvents.Should().ContainSingle(e => e is MaintenanceScheduleCreated);
    }

    [Fact]
    public void Create_WithBlankDescription_Throws()
    {
        var act = () => MaintenanceSchedule.Create(Company, Asset, MaintenanceScheduleFrequency.Monthly, "  ", NextDueDate);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyAssetId_Throws()
    {
        var act = () => MaintenanceSchedule.Create(Company, Guid.Empty, MaintenanceScheduleFrequency.Monthly, "Oil change", NextDueDate);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateDetails_ChangesFrequencyDescriptionAndNextDueDate()
    {
        var schedule = New();
        var newDueDate = NextDueDate.AddDays(90);

        schedule.UpdateDetails(MaintenanceScheduleFrequency.Annual, "Full inspection", newDueDate);

        schedule.Frequency.Should().Be(MaintenanceScheduleFrequency.Annual);
        schedule.Description.Should().Be("Full inspection");
        schedule.NextDueDate.Should().Be(newDueDate);
    }

    [Fact]
    public void UpdateDetails_WithBlankDescription_Throws()
    {
        var schedule = New();

        var act = () => schedule.UpdateDetails(MaintenanceScheduleFrequency.Annual, " ", NextDueDate);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Deactivate_SetsIsActiveFalse()
    {
        var schedule = New();

        schedule.Deactivate();

        schedule.IsActive.Should().BeFalse();
    }
}
