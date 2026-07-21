using FluentAssertions;
using FusionOS.Modules.Quality.Application.CorrectiveActions.Commands.CloseCorrectiveAction;
using FusionOS.Modules.Quality.Application.CorrectiveActions.Commands.CreateCorrectiveAction;
using FusionOS.Modules.Quality.Application.CorrectiveActions.Commands.StartCorrectiveAction;
using FusionOS.Modules.Quality.Application.CorrectiveActions.Commands.VerifyCorrectiveAction;
using FusionOS.Modules.Quality.Application.CorrectiveActions.Contracts;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using FusionOS.Modules.Quality.Application.NonConformanceReports.Contracts;
using FusionOS.Modules.Quality.Domain.CorrectiveActions;
using FusionOS.Modules.Quality.Domain.NonConformanceReports;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Quality.Tests.CorrectiveActions;

public class CorrectiveActionCommandHandlerTests
{
    private static readonly DateTimeOffset DueDate = DateTimeOffset.UtcNow.AddDays(7);

    [Fact]
    public async Task CreateCorrectiveAction_ValidatesNonConformanceReportExists_AndPersists()
    {
        var companyId = Guid.NewGuid();
        var ncr = NonConformanceReport.Create(companyId, null, "desc", NonConformanceReportSeverity.Major, Guid.NewGuid());
        var repository = Substitute.For<ICorrectiveActionRepository>();
        var ncrRepository = Substitute.For<INonConformanceReportRepository>();
        ncrRepository.GetByIdAsync(companyId, ncr.Id, Arg.Any<CancellationToken>()).Returns(ncr);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateCorrectiveActionCommandHandler(repository, ncrRepository, unitOfWork);

        var result = await handler.Handle(
            new CreateCorrectiveActionCommand(companyId, ncr.Id, "root", "fix", "prevent", Guid.NewGuid(), DueDate),
            CancellationToken.None);

        result.Status.Should().Be("Open");
        result.NonConformanceReportId.Should().Be(ncr.Id);
        await repository.Received(1).AddAsync(Arg.Any<CorrectiveAction>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateCorrectiveAction_NonConformanceReportMissing_Throws()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<ICorrectiveActionRepository>();
        var ncrRepository = Substitute.For<INonConformanceReportRepository>();
        ncrRepository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((NonConformanceReport?)null);
        var handler = new CreateCorrectiveActionCommandHandler(repository, ncrRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(
            new CreateCorrectiveActionCommand(companyId, Guid.NewGuid(), "root", "fix", "prevent", Guid.NewGuid(), DueDate),
            CancellationToken.None);

        await act.Should().ThrowAsync<FusionOS.BuildingBlocks.Application.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task Start_MovesToInProgress()
    {
        var companyId = Guid.NewGuid();
        var capa = CorrectiveAction.Create(companyId, Guid.NewGuid(), "root", "fix", "prevent", Guid.NewGuid(), DueDate);
        var repository = Substitute.For<ICorrectiveActionRepository>();
        repository.GetByIdAsync(companyId, capa.Id, Arg.Any<CancellationToken>()).Returns(capa);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new StartCorrectiveActionCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new StartCorrectiveActionCommand(companyId, capa.Id), CancellationToken.None);

        result.Status.Should().Be("InProgress");
    }

    [Fact]
    public async Task Close_WhenMissing_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<ICorrectiveActionRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((CorrectiveAction?)null);
        var handler = new CloseCorrectiveActionCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new CloseCorrectiveActionCommand(companyId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Verify_MovesToVerified()
    {
        var companyId = Guid.NewGuid();
        var capa = CorrectiveAction.Create(companyId, Guid.NewGuid(), "root", "fix", "prevent", Guid.NewGuid(), DueDate);
        capa.Start();
        capa.Close();
        var repository = Substitute.For<ICorrectiveActionRepository>();
        repository.GetByIdAsync(companyId, capa.Id, Arg.Any<CancellationToken>()).Returns(capa);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new VerifyCorrectiveActionCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new VerifyCorrectiveActionCommand(companyId, capa.Id), CancellationToken.None);

        result.Status.Should().Be("Verified");
    }
}
