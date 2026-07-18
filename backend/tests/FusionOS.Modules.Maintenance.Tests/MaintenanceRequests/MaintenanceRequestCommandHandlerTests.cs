using FluentAssertions;
using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.CompleteMaintenanceRequest;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.CreateMaintenanceRequest;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.StartMaintenanceRequest;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;
using FusionOS.Modules.Maintenance.Domain.MaintenanceRequests;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Maintenance.Tests.MaintenanceRequests;

public class MaintenanceRequestCommandHandlerTests
{
    [Fact]
    public async Task CreateMaintenanceRequest_WhenAssetExists_PersistsOpenRequest()
    {
        var companyId = Guid.NewGuid();
        var assetId = Guid.NewGuid();
        var repository = Substitute.For<IMaintenanceRequestRepository>();
        var assetRepository = Substitute.For<IAssetRepository>();
        assetRepository.ExistsAsync(companyId, assetId, Arg.Any<CancellationToken>()).Returns(true);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateMaintenanceRequestCommandHandler(repository, assetRepository, unitOfWork);

        var result = await handler.Handle(
            new CreateMaintenanceRequestCommand(companyId, assetId, MaintenanceRequestType.Breakdown, "Motor overheating"),
            CancellationToken.None);

        result.Status.Should().Be("Open");
        await repository.Received(1).AddAsync(Arg.Any<Domain.MaintenanceRequests.MaintenanceRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateMaintenanceRequest_WhenAssetMissing_ThrowsValidation()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IMaintenanceRequestRepository>();
        var assetRepository = Substitute.For<IAssetRepository>();
        assetRepository.ExistsAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var handler = new CreateMaintenanceRequestCommandHandler(repository, assetRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(
            new CreateMaintenanceRequestCommand(companyId, Guid.NewGuid(), MaintenanceRequestType.Preventive, "Scheduled service"),
            CancellationToken.None);

        await act.Should().ThrowAsync<FusionOS.BuildingBlocks.Application.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task StartThenComplete_ResolvesRequest()
    {
        var companyId = Guid.NewGuid();
        var request = Domain.MaintenanceRequests.MaintenanceRequest.Create(companyId, Guid.NewGuid(), MaintenanceRequestType.Breakdown, "Motor overheating");
        var repository = Substitute.For<IMaintenanceRequestRepository>();
        repository.GetByIdAsync(companyId, request.Id, Arg.Any<CancellationToken>()).Returns(request);
        var unitOfWork = Substitute.For<IUnitOfWork>();

        var startHandler = new StartMaintenanceRequestCommandHandler(repository, unitOfWork);
        var started = await startHandler.Handle(new StartMaintenanceRequestCommand(companyId, request.Id), CancellationToken.None);
        started.Status.Should().Be("InProgress");

        var completeHandler = new CompleteMaintenanceRequestCommandHandler(repository, unitOfWork);
        var completed = await completeHandler.Handle(new CompleteMaintenanceRequestCommand(companyId, request.Id, "Replaced bearing"), CancellationToken.None);
        completed.Status.Should().Be("Completed");
        completed.ResolutionNotes.Should().Be("Replaced bearing");
    }
}
