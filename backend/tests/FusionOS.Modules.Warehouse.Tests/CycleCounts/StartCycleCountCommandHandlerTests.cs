using FusionOS.Modules.Warehouse.Application.CycleCounts.Commands.StartCycleCount;
using FusionOS.Modules.Warehouse.Application.CycleCounts.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Domain.CycleCounts;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.SharedKernel.Context;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.CycleCounts;

public class StartCycleCountCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenBinExists_PersistsPendingCycleCount()
    {
        var userId = Guid.NewGuid();
        var repository = Substitute.For<ICycleCountRepository>();
        repository.BinExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(userId);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new StartCycleCountCommandHandler(repository, currentUser, unitOfWork);
        var command = new StartCycleCountCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(nameof(CycleCountStatus.Pending));
        result.StartedBy.Should().Be(userId);
        await repository.Received(1).AddAsync(Arg.Any<CycleCount>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenBinDoesNotExist_Throws()
    {
        var repository = Substitute.For<ICycleCountRepository>();
        repository.BinExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns(Guid.NewGuid());
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new StartCycleCountCommandHandler(repository, currentUser, unitOfWork);
        var command = new StartCycleCountCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenNoAuthenticatedUser_Throws()
    {
        var repository = Substitute.For<ICycleCountRepository>();
        var currentUser = Substitute.For<ICurrentUserContext>();
        currentUser.UserId.Returns((Guid?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new StartCycleCountCommandHandler(repository, currentUser, unitOfWork);
        var command = new StartCycleCountCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
