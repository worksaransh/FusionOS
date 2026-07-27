using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Commands.ReconcileMigrationJob;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;
using FusionOS.Modules.IntegrationHub.Domain.Migrations;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.IntegrationHub.Tests.Migrations;

/// <summary>
/// ReconcileMigrationJobCommandHandler had zero test coverage. Its one piece of real logic is
/// the "does loaded+failed account for everything extracted" match/mismatch note the handler
/// builds itself (not delegated to the domain) — easy to silently break with an off-by-one.
/// </summary>
public class MigrationJobCommandHandlerTests
{
    private static readonly Guid Company = Guid.NewGuid();

    private static MigrationJob CreateCompletedJob(int extracted, int loaded, int failed)
    {
        var job = MigrationJob.Create(Company, "Tally");
        job.StartExtraction();
        job.CompleteExtraction(extracted);
        job.StartValidation();
        job.CompleteValidation();
        job.StartLoad();
        job.CompleteLoad(loaded, failed);
        return job;
    }

    [Fact]
    public async Task Reconcile_WhenLoadedPlusFailedMatchesExtracted_NotesReconciliationMatches()
    {
        var job = CreateCompletedJob(extracted: 100, loaded: 92, failed: 8);
        var repository = Substitute.For<IMigrationJobRepository>();
        repository.GetByIdAsync(Company, job.Id, Arg.Any<CancellationToken>()).Returns(job);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ReconcileMigrationJobCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new ReconcileMigrationJobCommand(Company, job.Id), CancellationToken.None);

        result.ReconciliationNote.Should().Contain("92/100");
        result.ReconciliationNote.Should().Contain("Reconciliation matches.");
        result.ReconciliationNote.Should().NotContain("MISMATCH");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Reconcile_WhenLoadedPlusFailedUndershootsExtracted_NotesMismatch()
    {
        // 92 + 5 = 97, short of the 100 extracted — 3 records vanished somewhere between
        // extraction and load, which must be surfaced as a mismatch, not silently accepted.
        var job = CreateCompletedJob(extracted: 100, loaded: 92, failed: 5);
        var repository = Substitute.For<IMigrationJobRepository>();
        repository.GetByIdAsync(Company, job.Id, Arg.Any<CancellationToken>()).Returns(job);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ReconcileMigrationJobCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new ReconcileMigrationJobCommand(Company, job.Id), CancellationToken.None);

        result.ReconciliationNote.Should().Contain("MISMATCH");
    }

    [Fact]
    public async Task Reconcile_AllRecordsFailed_StillMatchesBecauseFailedCountsAsAccountedFor()
    {
        // Every extracted record failed validation/load — loaded=0, failed=extracted. The
        // handler's definition of "accounted for" is loaded+failed, so this is still a match
        // (100% failure rate is a real, honestly-reported result, not a reconciliation mismatch).
        var job = CreateCompletedJob(extracted: 50, loaded: 0, failed: 50);
        var repository = Substitute.For<IMigrationJobRepository>();
        repository.GetByIdAsync(Company, job.Id, Arg.Any<CancellationToken>()).Returns(job);
        var handler = new ReconcileMigrationJobCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var result = await handler.Handle(new ReconcileMigrationJobCommand(Company, job.Id), CancellationToken.None);

        result.ReconciliationNote.Should().Contain("0/50");
        result.ReconciliationNote.Should().Contain("Reconciliation matches.");
    }

    [Fact]
    public async Task Reconcile_WhenJobDoesNotExist_ThrowsValidationException()
    {
        var repository = Substitute.For<IMigrationJobRepository>();
        repository.GetByIdAsync(Company, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((MigrationJob?)null);
        var handler = new ReconcileMigrationJobCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new ReconcileMigrationJobCommand(Company, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Reconcile_WhenJobNotYetCompleted_PropagatesDomainInvalidOperationException()
    {
        // The job never left "Created" — Reconcile() requires Status == Completed first;
        // the handler does not catch/translate this one, so the raw domain exception surfaces.
        var job = MigrationJob.Create(Company, "Tally");
        var repository = Substitute.For<IMigrationJobRepository>();
        repository.GetByIdAsync(Company, job.Id, Arg.Any<CancellationToken>()).Returns(job);
        var handler = new ReconcileMigrationJobCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new ReconcileMigrationJobCommand(Company, job.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
