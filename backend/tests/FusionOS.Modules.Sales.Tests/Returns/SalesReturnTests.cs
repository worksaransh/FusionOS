using FusionOS.Modules.Sales.Domain.Returns;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Returns;

/// <summary>
/// Covers SalesReturn's Requested -> Approved/Rejected -> Completed state machine —
/// same "illegal transition throws InvalidOperationException" shape as
/// DispatchTests/IssueCreditNoteCommandHandlerTests, but exercised directly against
/// the aggregate (not through a handler) since Approve/Reject/Complete are pure
/// domain-level transitions.
/// </summary>
public class SalesReturnTests
{
    private static SalesReturn CreateRequested(out Guid lineId)
    {
        var salesReturn = SalesReturn.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Damaged goods", new[] { new SalesReturnLineInput(Guid.NewGuid(), 5m) });
        lineId = salesReturn.Lines[0].Id;
        return salesReturn;
    }

    [Fact]
    public void Create_SetsStatusToRequested()
    {
        var salesReturn = CreateRequested(out _);

        salesReturn.Status.Should().Be(SalesReturnStatus.Requested);
    }

    [Fact]
    public void Create_WithNoLines_Throws()
    {
        var act = () => SalesReturn.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Damaged goods", Array.Empty<SalesReturnLineInput>());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithoutReason_Throws()
    {
        var act = () => SalesReturn.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "  ", new[] { new SalesReturnLineInput(Guid.NewGuid(), 5m) });

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Approve_WhenRequested_TransitionsToApproved()
    {
        var salesReturn = CreateRequested(out _);

        salesReturn.Approve();

        salesReturn.Status.Should().Be(SalesReturnStatus.Approved);
    }

    [Fact]
    public void Approve_WhenNotRequested_Throws()
    {
        var salesReturn = CreateRequested(out _);
        salesReturn.Approve();

        var act = () => salesReturn.Approve();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Reject_WhenRequested_TransitionsToRejectedAndRecordsReason()
    {
        var salesReturn = CreateRequested(out _);

        salesReturn.Reject("Outside the return window");

        salesReturn.Status.Should().Be(SalesReturnStatus.Rejected);
        salesReturn.RejectionReason.Should().Be("Outside the return window");
    }

    [Fact]
    public void Reject_WithoutReason_Throws()
    {
        var salesReturn = CreateRequested(out _);

        var act = () => salesReturn.Reject("  ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Reject_WhenAlreadyApproved_Throws()
    {
        var salesReturn = CreateRequested(out _);
        salesReturn.Approve();

        var act = () => salesReturn.Reject("Too late");

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Complete_WhenApproved_TransitionsToCompletedAndAppliesLineCompletions()
    {
        var salesReturn = CreateRequested(out var lineId);
        salesReturn.Approve();

        salesReturn.Complete(new[] { new SalesReturnLineCompletion(lineId, true, "Unopened, resellable") });

        salesReturn.Status.Should().Be(SalesReturnStatus.Completed);
        salesReturn.Lines[0].Restocked.Should().BeTrue();
        salesReturn.Lines[0].ConditionNotes.Should().Be("Unopened, resellable");
    }

    [Fact]
    public void Complete_WhenStillRequested_Throws()
    {
        var salesReturn = CreateRequested(out var lineId);

        var act = () => salesReturn.Complete(new[] { new SalesReturnLineCompletion(lineId, true, null) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Complete_WhenAlreadyCompleted_Throws()
    {
        var salesReturn = CreateRequested(out var lineId);
        salesReturn.Approve();
        salesReturn.Complete(new[] { new SalesReturnLineCompletion(lineId, false, "Damaged") });

        var act = () => salesReturn.Complete(new[] { new SalesReturnLineCompletion(lineId, true, null) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Complete_WhenRejected_Throws()
    {
        var salesReturn = CreateRequested(out var lineId);
        salesReturn.Reject("Not eligible");

        var act = () => salesReturn.Complete(new[] { new SalesReturnLineCompletion(lineId, true, null) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Complete_WithMissingLineCompletion_Throws()
    {
        var salesReturn = CreateRequested(out _);
        salesReturn.Approve();

        var act = () => salesReturn.Complete(Array.Empty<SalesReturnLineCompletion>());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Complete_WithUnknownLineId_Throws()
    {
        var salesReturn = CreateRequested(out _);
        salesReturn.Approve();

        var act = () => salesReturn.Complete(new[] { new SalesReturnLineCompletion(Guid.NewGuid(), true, null) });

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Complete_WithDuplicateLineCompletion_Throws()
    {
        var salesReturn = CreateRequested(out var lineId);
        salesReturn.Approve();

        var act = () => salesReturn.Complete(new[]
        {
            new SalesReturnLineCompletion(lineId, true, null),
            new SalesReturnLineCompletion(lineId, false, "Damaged"),
        });

        act.Should().Throw<ArgumentException>();
    }
}
