using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.JournalEntries.Commands.PostJournalEntry;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;
using FusionOS.Modules.Finance.Domain.JournalEntries;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.JournalEntries;

public class PostJournalEntryCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenEntryExists_PostsIt()
    {
        var companyId = Guid.NewGuid();
        var entry = JournalEntry.Create(companyId, "REF-1", new[]
        {
            new JournalEntryLineInput(Guid.NewGuid(), 100m, 0m, null),
            new JournalEntryLineInput(Guid.NewGuid(), 0m, 100m, null),
        });

        var repository = Substitute.For<IJournalEntryRepository>();
        repository.GetByIdAsync(companyId, entry.Id, Arg.Any<CancellationToken>()).Returns(entry);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new PostJournalEntryCommandHandler(repository, unitOfWork);
        var command = new PostJournalEntryCommand(companyId, entry.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Status.Should().Be(nameof(JournalEntryStatus.Posted));
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEntryDoesNotExist_Throws()
    {
        var repository = Substitute.For<IJournalEntryRepository>();
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((JournalEntry?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new PostJournalEntryCommandHandler(repository, unitOfWork);
        var command = new PostJournalEntryCommand(Guid.NewGuid(), Guid.NewGuid());

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
