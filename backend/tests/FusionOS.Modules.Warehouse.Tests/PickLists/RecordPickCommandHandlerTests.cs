using FusionOS.Modules.Warehouse.Application.PickLists.Commands.RecordPick;
using FusionOS.Modules.Warehouse.Application.PickLists.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Domain.PickLists;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.PickLists;

public class RecordPickCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenAssignedAndWithinQuantity_RecordsAndSaves()
    {
        var companyId = Guid.NewGuid();
        var pickList = Domain.PickLists.PickList.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), new[] { new PickListLineInput(Guid.NewGuid(), null, 10m) });
        pickList.AssignTo(Guid.NewGuid());
        var repository = Substitute.For<IPickListRepository>();
        repository.GetByIdAsync(pickList.Id, Arg.Any<CancellationToken>()).Returns(pickList);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordPickCommandHandler(repository, unitOfWork);
        var command = new RecordPickCommand(companyId, pickList.Id, pickList.Lines[0].Id, 10m);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(nameof(PickListStatus.Picked));
        result.Lines[0].QuantityPicked.Should().Be(10m);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPickListNotFound_Throws()
    {
        var repository = Substitute.For<IPickListRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Domain.PickLists.PickList?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordPickCommandHandler(repository, unitOfWork);
        var command = new RecordPickCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5m);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenNotYetAssigned_Throws()
    {
        var companyId = Guid.NewGuid();
        var pickList = Domain.PickLists.PickList.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), new[] { new PickListLineInput(Guid.NewGuid(), null, 10m) });
        var repository = Substitute.For<IPickListRepository>();
        repository.GetByIdAsync(pickList.Id, Arg.Any<CancellationToken>()).Returns(pickList);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordPickCommandHandler(repository, unitOfWork);
        var command = new RecordPickCommand(companyId, pickList.Id, pickList.Lines[0].Id, 5m);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
