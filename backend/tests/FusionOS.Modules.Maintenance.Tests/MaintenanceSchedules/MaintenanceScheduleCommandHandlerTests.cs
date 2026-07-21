using FluentAssertions;
using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Commands.CreateMaintenanceSchedule;
using FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Commands.DeactivateMaintenanceSchedule;
using FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Commands.UpdateMaintenanceSchedule;
using FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Contracts;
using FusionOS.Modules.Maintenance.Domain.MaintenanceSchedules;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Maintenance.Tests.MaintenanceSchedules;

public class MaintenanceScheduleCommandHandlerTests
{
    [Fact]
    public async Task CreateMaintenanceSchedule_WhenAssetExists_PersistsActiveSchedule()
    {
        var companyId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var repository = Substitute.For<IMaintenanceScheduleRepository>();
        var assetRepository = Substitute.For<IAssetRepository>();
        assetRepository.ExistsAsync(companyId, assetId, Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateMaintenanceScheduleCommandHandler(repository, assetRepository, unitOfWork);

        var result = await handler.Handle(
            new CreateMaintenanceScheduleCommand(companyId, assetId, MaintenanceScheduleFrequency.Monthly, "Filter replacement", DateTimeOffset.UtcNow.AddDays(30)),
            CancellationToken.None);

        result.IsActive.Should().BeTrue();
        result.Frequency.Should().Be("Monthly");
        await repository.Received(1).AddAsync(Arg.Any<Domain.MaintenanceSchedules.MaintenanceSchedule>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateMaintenanceSchedule_WhenAssetMissing_ThrowsValidation()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IMaintenanceScheduleRepository>();
        var assetRepository = Substitute.For<IAssetRepository>();
        assetRepository.ExistsAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var handler = new CreateMaintenanceScheduleCommandHandler(repository, assetRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(
            new CreateMaintenanceScheduleCommand(companyId, Guid.NewGuid(), MaintenanceScheduleFrequency.Weekly, "Lubrication", DateTimeOffset.UtcNow.AddDays(7)),
            CancellationToken.None);

        await act.Should().ThrowAsync<FusionOS.BuildingBlocks.Application.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task UpdateThenDeactivate_ChangesScheduleAndDeactivates()
    {
        var companyId = Guid.NewGuid();
        var schedule = Domain.MaintenanceSchedules.MaintenanceSchedule.Create(companyId, Guid.NewGuid(), MaintenanceScheduleFrequency.Monthly, "Filter replacement", DateTimeOffset.UtcNow.AddDays(30));
        var repository = Substitute.For<IMaintenanceScheduleRepository>();
        repository.GetByIdAsync(companyId, schedule.Id, Arg.Any<CancellationToken>()).Returns(schedule);
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var newDueDate = DateTimeOffset.UtcNow.AddDays(90);
        var updateHandler = new UpdateMaintenanceScheduleCommandHandler(repository, unitOfWork);
        var updated = await updateHandler.Handle(
            new UpdateMaintenanceScheduleCommand(companyId, schedule.Id, MaintenanceScheduleFrequency.Quarterly, "Full service", newDueDate),
            CancellationToken.None);
        updated.Frequency.Should().Be("Quarterly");
        updated.Description.Should().Be("Full service");
        updated.NextDueDate.Should().Be(newDueDate);

        var deactivateHandler = new DeactivateMaintenanceScheduleCommandHandler(repository, unitOfWork);
        var deactivated = await deactivateHandler.Handle(new DeactivateMaintenanceScheduleCommand(companyId, schedule.Id), CancellationToken.None);
        deactivated.IsActive.Should().BeFalse();
    }
}
