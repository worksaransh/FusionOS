using FusionOS.Modules.Warehouse.Application.CycleCounts.Commands.RecordCycleCount;
using FusionOS.Modules.Warehouse.Application.CycleCounts.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Domain.CycleCounts;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.CycleCounts;

public class RecordCycleCountCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithVariance_CompletesAndSaves()
    {
        var companyId = Guid.NewGuid();
        var cycleCount = CycleCount.Start(companyId, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m, Guid.NewGuid());
        var repository = Substitute.For<ICycleCountRepository>();
        repository.GetByIdAsync(cycleCount.Id, Arg.Any<CancellationToken>()).Returns(cycleCount);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordCycleCountCommandHandler(repository, unitOfWork);
        var command = new RecordCycleCountCommand(companyId, cycleCount.Id, 92m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(nameof(CycleCountStatus.Completed));
        result.VarianceQuantity.Should().Be(-8m);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCycleCountNotFound_Throws()
    {
        var repository = Substitute.For<ICycleCountRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((CycleCount?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordCycleCountCommandHandler(repository, unitOfWork);
        var command = new RecordCycleCountCommand(Guid.NewGuid(), Guid.NewGuid(), 92m);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCycleCountBelongsToDifferentCompany_Throws()
    {
        var cycleCount = CycleCount.Start(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 100m, Guid.NewGuid());
        var repository = Substitute.For<ICycleCountRepository>();
        repository.GetByIdAsync(cycleCount.Id, Arg.Any<CancellationToken>()).Returns(cycleCount);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordCycleCountCommandHandler(repository, unitOfWork);
        var command = new RecordCycleCountCommand(Guid.NewGuid(), cycleCount.Id, 92m);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
