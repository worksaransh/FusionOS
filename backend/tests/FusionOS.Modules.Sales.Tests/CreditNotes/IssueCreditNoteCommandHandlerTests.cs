using FusionOS.Modules.Sales.Application.CreditNotes.Commands.IssueCreditNote;
using FusionOS.Modules.Sales.Application.CreditNotes.Contracts;
using FusionOS.Modules.Sales.Domain.CreditNotes;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.CreditNotes;

public class IssueCreditNoteCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenCreditNoteIsDraft_IssuesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var creditNote = CreditNote.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), "Damaged goods", new[] { new CreditNoteLineInput(Guid.NewGuid(), 2m, 10m) });
        var repository = Substitute.For<ICreditNoteRepository>();
        repository.GetByIdAsync(companyId, creditNote.Id, Arg.Any<CancellationToken>()).Returns(creditNote);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new IssueCreditNoteCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new IssueCreditNoteCommand(companyId, creditNote.Id), CancellationToken.None);

        result.Status.Should().Be(nameof(CreditNoteStatus.Issued));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCreditNoteIsAlreadyIssued_ThrowsInvalidOperationException()
    {
        var companyId = Guid.NewGuid();
        var creditNote = CreditNote.Create(companyId, Guid.NewGuid(), Guid.NewGuid(), "Damaged goods", new[] { new CreditNoteLineInput(Guid.NewGuid(), 2m, 10m) });
        var repository = Substitute.For<ICreditNoteRepository>();
        repository.GetByIdAsync(companyId, creditNote.Id, Arg.Any<CancellationToken>()).Returns(creditNote);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new IssueCreditNoteCommandHandler(repository, unitOfWork);
        await handler.Handle(new IssueCreditNoteCommand(companyId, creditNote.Id), CancellationToken.None);

        var act = () => handler.Handle(new IssueCreditNoteCommand(companyId, creditNote.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenCreditNoteDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var creditNoteId = Guid.NewGuid();
        var repository = Substitute.For<ICreditNoteRepository>();
        repository.GetByIdAsync(companyId, creditNoteId, Arg.Any<CancellationToken>()).Returns((CreditNote?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new IssueCreditNoteCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new IssueCreditNoteCommand(companyId, creditNoteId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
