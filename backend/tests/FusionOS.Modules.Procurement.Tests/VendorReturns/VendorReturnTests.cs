using FusionOS.Modules.Procurement.Domain.VendorReturns;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.VendorReturns;

public class VendorReturnTests
{
    [Fact]
    public void Create_WithValidInputs_StartsPending()
    {
        var vendorReturn = VendorReturn.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5m, "Damaged in transit");

        vendorReturn.Status.Should().Be(VendorReturnStatus.Pending);
        vendorReturn.DomainEvents.Should().ContainSingle(e => e is Events.VendorReturnCreated);
    }

    [Fact]
    public void Create_WithNonPositiveQuantity_Throws()
    {
        var act = () => VendorReturn.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 0m, "Damaged");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithoutReason_Throws()
    {
        var act = () => VendorReturn.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5m, "  ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Complete_FromPending_TransitionsToCompletedAndRaisesEvent()
    {
        var vendorReturn = VendorReturn.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5m, "Damaged");
        vendorReturn.ClearDomainEvents();

        vendorReturn.Complete();

        vendorReturn.Status.Should().Be(VendorReturnStatus.Completed);
        vendorReturn.DomainEvents.Should().ContainSingle(e => e is Events.VendorReturnCompleted);
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_Throws()
    {
        var vendorReturn = VendorReturn.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5m, "Damaged");
        vendorReturn.Complete();

        var act = () => vendorReturn.Complete();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Cancel_FromPending_TransitionsToCancelled()
    {
        var vendorReturn = VendorReturn.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5m, "Damaged");

        vendorReturn.Cancel();

        vendorReturn.Status.Should().Be(VendorReturnStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenAlreadyCompleted_Throws()
    {
        var vendorReturn = VendorReturn.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 5m, "Damaged");
        vendorReturn.Complete();

        var act = () => vendorReturn.Cancel();

        act.Should().Throw<InvalidOperationException>();
    }
}
