using FusionOS.Modules.Warehouse.Application.PickLists.Commands.PackPickList;
using FusionOS.Modules.Warehouse.Application.PickLists.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Domain.PickLists;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.PickLists;

public class PackPickListCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenFullyPicked_PacksAndSaves()
    {
        var companyId = Guid.NewGuid();
        var pickList = Domain.PickLists.PickList.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), new[] { new PickListLineInput(Guid.NewGuid(), null, 10m) });
        pickList.AssignTo(Guid.NewGuid());
        pickList.RecordPick(pickList.Lines[0].Id, 10m);
        var repository = Substitute.For<IPickListRepository>();
        repository.GetByIdAsync(pickList.Id, Arg.Any<CancellationToken>()).Returns(pickList);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new PackPickListCommandHandler(repository, unitOfWork);
        var command = new PackPickListCommand(companyId, pickList.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(nameof(PickListStatus.Packed));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNotFullyPicked_ThrowsAndDoesNotSave()
    {
        var companyId = Guid.NewGuid();
        var pickList = Domain.PickLists.PickList.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), new[] { new PickListLineInput(Guid.NewGuid(), null, 10m) });
        var repository = Substitute.For<IPickListRepository>();
        repository.GetByIdAsync(pickList.Id, Arg.Any<CancellationToken>()).Returns(pickList);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new PackPickListCommandHandler(repository, unitOfWork);
        var command = new PackPickListCommand(companyId, pickList.Id);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPickListNotFound_Throws()
    {
        var repository = Substitute.For<IPickListRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Domain.PickLists.PickList?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new PackPickListCommandHandler(repository, unitOfWork);
        var command = new PackPickListCommand(Guid.NewGuid(), Guid.NewGuid());

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
