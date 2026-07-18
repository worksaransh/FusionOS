using FusionOS.Modules.Warehouse.Application.PickLists.Commands.AssignPickList;
using FusionOS.Modules.Warehouse.Application.PickLists.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using FusionOS.Modules.Warehouse.Domain.PickLists;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Warehouse.Tests.PickLists;

public class AssignPickListCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenPickListExists_AssignsAndSaves()
    {
        var companyId = Guid.NewGuid();
        var pickList = Domain.PickLists.PickList.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), new[] { new PickListLineInput(Guid.NewGuid(), null, 5m) });
        var repository = Substitute.For<IPickListRepository>();
        repository.GetByIdAsync(pickList.Id, Arg.Any<CancellationToken>()).Returns(pickList);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AssignPickListCommandHandler(repository, unitOfWork);
        var assignee = Guid.NewGuid();
        var command = new AssignPickListCommand(companyId, pickList.Id, assignee);

        var result = await handler.Handle(command, CancellationToken.None);

        result.AssignedToUserId.Should().Be(assignee);
        result.Status.Should().Be(nameof(PickListStatus.Assigned));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPickListNotFound_Throws()
    {
        var repository = Substitute.For<IPickListRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Domain.PickLists.PickList?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new AssignPickListCommandHandler(repository, unitOfWork);
        var command = new AssignPickListCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
