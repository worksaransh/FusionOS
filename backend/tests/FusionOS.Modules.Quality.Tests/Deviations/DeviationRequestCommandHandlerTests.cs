using FluentAssertions;
using FusionOS.Modules.Quality.Application.Deviations.Commands.ApproveDeviationRequest;
using FusionOS.Modules.Quality.Application.Deviations.Commands.RejectDeviationRequest;
using FusionOS.Modules.Quality.Application.Deviations.Contracts;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using FusionOS.Modules.Quality.Domain.Deviations;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Quality.Tests.Deviations;

/// <summary>
/// ApproveDeviationRequestCommandHandler and RejectDeviationRequestCommandHandler had zero
/// test coverage. Both wrap DeviationRequest's one-shot Pending -> Approved/Rejected decision
/// (DeviationRequest.cs) — a terminal decision that cannot be revisited, so the
/// "already decided" guard is the risky edge case worth locking down.
/// </summary>
public class DeviationRequestCommandHandlerTests
{
    private static readonly Guid Company = Guid.NewGuid();

    private static DeviationRequest CreatePending() =>
        DeviationRequest.Create(Company, null, "Batch ran 2C outside the specified temperature range.", Guid.NewGuid());

    [Fact]
    public async Task Approve_FromPending_TransitionsToApprovedWithDecisionNote()
    {
        var request = CreatePending();
        var decidedBy = Guid.NewGuid();
        var repository = Substitute.For<IDeviationRequestRepository>();
        repository.GetByIdAsync(Company, request.Id, Arg.Any<CancellationToken>()).Returns(request);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ApproveDeviationRequestCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new ApproveDeviationRequestCommand(Company, request.Id, decidedBy, "Acceptable per QA-114."), CancellationToken.None);

        result.Status.Should().Be("Approved");
        result.ApprovedOrRejectedByUserId.Should().Be(decidedBy);
        result.DecisionNote.Should().Be("Acceptable per QA-114.");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Approve_AlreadyDecided_Throws()
    {
        var request = CreatePending();
        request.Approve(Guid.NewGuid(), null);
        var repository = Substitute.For<IDeviationRequestRepository>();
        repository.GetByIdAsync(Company, request.Id, Arg.Any<CancellationToken>()).Returns(request);
        var handler = new ApproveDeviationRequestCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new ApproveDeviationRequestCommand(Company, request.Id, Guid.NewGuid(), null), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Approve_WhenMissing_ThrowsKeyNotFound()
    {
        var repository = Substitute.For<IDeviationRequestRepository>();
        repository.GetByIdAsync(Company, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((DeviationRequest?)null);
        var handler = new ApproveDeviationRequestCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new ApproveDeviationRequestCommand(Company, Guid.NewGuid(), Guid.NewGuid(), null), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Reject_FromPending_TransitionsToRejected()
    {
        var request = CreatePending();
        var decidedBy = Guid.NewGuid();
        var repository = Substitute.For<IDeviationRequestRepository>();
        repository.GetByIdAsync(Company, request.Id, Arg.Any<CancellationToken>()).Returns(request);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RejectDeviationRequestCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new RejectDeviationRequestCommand(Company, request.Id, decidedBy, "Out of tolerance, no compensating control."), CancellationToken.None);

        result.Status.Should().Be("Rejected");
        result.ApprovedOrRejectedByUserId.Should().Be(decidedBy);
    }

    [Fact]
    public async Task Reject_AfterAlreadyApproved_Throws()
    {
        // A decision, once made either way, is terminal — rejecting an already-approved
        // request must fail rather than silently flip the decision.
        var request = CreatePending();
        request.Approve(Guid.NewGuid(), null);
        var repository = Substitute.For<IDeviationRequestRepository>();
        repository.GetByIdAsync(Company, request.Id, Arg.Any<CancellationToken>()).Returns(request);
        var handler = new RejectDeviationRequestCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new RejectDeviationRequestCommand(Company, request.Id, Guid.NewGuid(), null), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Reject_WhenMissing_ThrowsKeyNotFound()
    {
        var repository = Substitute.For<IDeviationRequestRepository>();
        repository.GetByIdAsync(Company, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((DeviationRequest?)null);
        var handler = new RejectDeviationRequestCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new RejectDeviationRequestCommand(Company, Guid.NewGuid(), Guid.NewGuid(), null), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
