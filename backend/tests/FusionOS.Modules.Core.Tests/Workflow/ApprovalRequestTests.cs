using FusionOS.Modules.Core.Domain.Workflow;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Core.Tests.Workflow;

/// <summary>Covers ApprovalRequest/ApprovalStep (Phase M7, 2026-07-15) — the generic multi-step approval engine.</summary>
public class ApprovalRequestTests
{
    [Fact]
    public void Submit_WithOneApprover_CreatesASingleStepPendingRequest()
    {
        var companyId = Guid.NewGuid();
        var requestedBy = Guid.NewGuid();
        var approverId = Guid.NewGuid();

        var request = ApprovalRequest.Submit(companyId, "PurchaseOrder", Guid.NewGuid(), requestedBy, new[] { approverId });

        request.Status.Should().Be(ApprovalStatus.Pending);
        request.CurrentStepNumber.Should().Be(1);
        request.Steps.Should().ContainSingle();
        request.Steps[0].ApproverUserId.Should().Be(approverId);
        request.Steps[0].Decision.Should().Be(ApprovalStatus.Pending);
    }

    [Fact]
    public void Submit_WithMultipleApprovers_CreatesStepsInOrder()
    {
        var approver1 = Guid.NewGuid();
        var approver2 = Guid.NewGuid();
        var approver3 = Guid.NewGuid();

        var request = ApprovalRequest.Submit(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), Guid.NewGuid(), new[] { approver1, approver2, approver3 });

        request.Steps.Should().HaveCount(3);
        request.Steps[0].StepNumber.Should().Be(1);
        request.Steps[1].StepNumber.Should().Be(2);
        request.Steps[2].StepNumber.Should().Be(3);
        request.Steps.Select(s => s.ApproverUserId).Should().ContainInOrder(approver1, approver2, approver3);
    }

    [Fact]
    public void Submit_WhenRequesterIsAlsoAnApprover_Throws()
    {
        var requestedBy = Guid.NewGuid();

        var act = () => ApprovalRequest.Submit(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), requestedBy, new[] { requestedBy });

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Submit_WithNoApprovers_Throws()
    {
        var act = () => ApprovalRequest.Submit(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), Guid.NewGuid(), Array.Empty<Guid>());

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Decide_ApprovingTheOnlyStep_CompletesTheRequestAsApproved()
    {
        var approverId = Guid.NewGuid();
        var request = ApprovalRequest.Submit(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), Guid.NewGuid(), new[] { approverId });

        request.Decide(approverId, approve: true, comments: "Looks good");

        request.Status.Should().Be(ApprovalStatus.Approved);
        request.Steps[0].Decision.Should().Be(ApprovalStatus.Approved);
        request.Steps[0].Comments.Should().Be("Looks good");
    }

    [Fact]
    public void Decide_ApprovingAnEarlierStep_AdvancesCurrentStepNumberAndLeavesRequestPending()
    {
        var approver1 = Guid.NewGuid();
        var approver2 = Guid.NewGuid();
        var request = ApprovalRequest.Submit(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), Guid.NewGuid(), new[] { approver1, approver2 });

        request.Decide(approver1, approve: true, comments: null);

        request.Status.Should().Be(ApprovalStatus.Pending);
        request.CurrentStepNumber.Should().Be(2);
        request.Steps[0].Decision.Should().Be(ApprovalStatus.Approved);
        request.Steps[1].Decision.Should().Be(ApprovalStatus.Pending);
    }

    [Fact]
    public void Decide_RejectingAnyStep_HaltsTheWholeChainImmediately()
    {
        var approver1 = Guid.NewGuid();
        var approver2 = Guid.NewGuid();
        var request = ApprovalRequest.Submit(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), Guid.NewGuid(), new[] { approver1, approver2 });

        request.Decide(approver1, approve: false, comments: "Not this one");

        request.Status.Should().Be(ApprovalStatus.Rejected);
        request.Steps[0].Decision.Should().Be(ApprovalStatus.Rejected);
        request.Steps[1].Decision.Should().Be(ApprovalStatus.Pending); // never reached
    }

    [Fact]
    public void Decide_ByAUserWhoIsNotTheCurrentStepApprover_Throws()
    {
        var actualApprover = Guid.NewGuid();
        var someoneElse = Guid.NewGuid();
        var request = ApprovalRequest.Submit(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), Guid.NewGuid(), new[] { actualApprover });

        var act = () => request.Decide(someoneElse, approve: true, comments: null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Decide_ByALaterStepsApproverBeforeTheirTurn_Throws()
    {
        var approver1 = Guid.NewGuid();
        var approver2 = Guid.NewGuid();
        var request = ApprovalRequest.Submit(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), Guid.NewGuid(), new[] { approver1, approver2 });

        // approver2 tries to decide step 1, which is still approver1's turn
        var act = () => request.Decide(approver2, approve: true, comments: null);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Decide_OnAnAlreadyDecidedRequest_Throws()
    {
        var approverId = Guid.NewGuid();
        var request = ApprovalRequest.Submit(Guid.NewGuid(), "PurchaseOrder", Guid.NewGuid(), Guid.NewGuid(), new[] { approverId });
        request.Decide(approverId, approve: true, comments: null);

        var act = () => request.Decide(approverId, approve: true, comments: null);

        act.Should().Throw<InvalidOperationException>();
    }
}
