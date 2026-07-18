using FusionOS.Modules.Inventory.Domain.Transfers;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Inventory.Tests.Transfers;

public class TransferTests
{
    [Fact]
    public void Create_WithValidInputs_StartsPending()
    {
        var transfer = Transfer.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10m);

        transfer.Status.Should().Be(TransferStatus.Pending);
        transfer.DomainEvents.Should().ContainSingle(e => e is Events.TransferCreated);
    }

    [Fact]
    public void Create_WithSameSourceAndDestination_Throws()
    {
        var warehouseId = Guid.NewGuid();

        var act = () => Transfer.Create(Guid.NewGuid(), Guid.NewGuid(), warehouseId, warehouseId, 10m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithNonPositiveQuantity_Throws()
    {
        var act = () => Transfer.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0m);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Complete_FromPending_TransitionsToCompletedAndRaisesEvent()
    {
        var transfer = Transfer.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10m);
        transfer.ClearDomainEvents();

        transfer.Complete();

        transfer.Status.Should().Be(TransferStatus.Completed);
        transfer.DomainEvents.Should().ContainSingle(e => e is Events.TransferCompleted);
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_Throws()
    {
        var transfer = Transfer.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10m);
        transfer.Complete();

        var act = () => transfer.Complete();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_FromPending_TransitionsToCancelled()
    {
        var transfer = Transfer.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10m);

        transfer.Cancel();

        transfer.Status.Should().Be(TransferStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenAlreadyCompleted_Throws()
    {
        var transfer = Transfer.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 10m);
        transfer.Complete();

        var act = () => transfer.Cancel();

        act.Should().Throw<InvalidOperationException>();
    }
}
