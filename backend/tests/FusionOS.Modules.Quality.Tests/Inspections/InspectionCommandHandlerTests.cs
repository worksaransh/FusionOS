using FluentAssertions;
using FusionOS.Modules.Quality.Application.Inspections.Commands.CreateInspection;
using FusionOS.Modules.Quality.Application.Inspections.Commands.RecordInspectionResults;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using FusionOS.Modules.Quality.Domain.Inspections;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Quality.Tests.Inspections;

public class InspectionCommandHandlerTests
{
    [Fact]
    public async Task CreateInspection_PersistsPendingInspection()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IInspectionRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateInspectionCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new CreateInspectionCommand(companyId, InspectionType.Production, Guid.NewGuid(), new[] { "Torque", "Finish" }),
            CancellationToken.None);

        result.Status.Should().Be("Pending");
        result.Items.Should().HaveCount(2);
        await repository.Received(1).AddAsync(Arg.Any<Inspection>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordResults_ResolvesInspection()
    {
        var companyId = Guid.NewGuid();
        var inspection = Inspection.Create(companyId, InspectionType.Production, Guid.NewGuid(), new[] { "Torque" });
        var repository = Substitute.For<IInspectionRepository>();
        repository.GetByIdAsync(companyId, inspection.Id, Arg.Any<CancellationToken>()).Returns(inspection);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new RecordInspectionResultsCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new RecordInspectionResultsCommand(companyId, inspection.Id, new[] { new InspectionResultInput("Torque", true, null) }),
            CancellationToken.None);

        result.Status.Should().Be("Passed");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RecordResults_WhenInspectionMissing_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IInspectionRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Inspection?)null);
        var handler = new RecordInspectionResultsCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(
            new RecordInspectionResultsCommand(companyId, Guid.NewGuid(), new[] { new InspectionResultInput("Torque", true, null) }),
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
