using FluentAssertions;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using FusionOS.Modules.Quality.Application.NonConformanceReports.Commands.CreateNonConformanceReport;
using FusionOS.Modules.Quality.Application.NonConformanceReports.Commands.UpdateNonConformanceReportStatus;
using FusionOS.Modules.Quality.Application.NonConformanceReports.Contracts;
using FusionOS.Modules.Quality.Domain.Inspections;
using FusionOS.Modules.Quality.Domain.NonConformanceReports;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Quality.Tests.NonConformanceReports;

public class NonConformanceReportCommandHandlerTests
{
    [Fact]
    public async Task CreateNonConformanceReport_Standalone_Persists()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<INonConformanceReportRepository>();
        var inspectionRepository = Substitute.For<IInspectionRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateNonConformanceReportCommandHandler(repository, inspectionRepository, unitOfWork);

        var result = await handler.Handle(
            new CreateNonConformanceReportCommand(companyId, null, "Weld crack", NonConformanceReportSeverity.Critical, Guid.NewGuid()),
            CancellationToken.None);

        result.Status.Should().Be("Open");
        result.InspectionId.Should().BeNull();
        await repository.Received(1).AddAsync(Arg.Any<NonConformanceReport>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateNonConformanceReport_WithInspectionId_ValidatesExistence()
    {
        var companyId = Guid.NewGuid();
        var inspection = Inspection.Create(companyId, InspectionType.Production, Guid.NewGuid(), new[] { "Torque" });
        var repository = Substitute.For<INonConformanceReportRepository>();
        var inspectionRepository = Substitute.For<IInspectionRepository>();
        inspectionRepository.GetByIdAsync(companyId, inspection.Id, Arg.Any<CancellationToken>()).Returns(inspection);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateNonConformanceReportCommandHandler(repository, inspectionRepository, unitOfWork);

        var result = await handler.Handle(
            new CreateNonConformanceReportCommand(companyId, inspection.Id, "Torque out of spec", NonConformanceReportSeverity.Major, Guid.NewGuid()),
            CancellationToken.None);

        result.InspectionId.Should().Be(inspection.Id);
    }

    [Fact]
    public async Task CreateNonConformanceReport_InspectionMissing_Throws()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<INonConformanceReportRepository>();
        var inspectionRepository = Substitute.For<IInspectionRepository>();
        inspectionRepository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Inspection?)null);
        var handler = new CreateNonConformanceReportCommandHandler(repository, inspectionRepository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(
            new CreateNonConformanceReportCommand(companyId, Guid.NewGuid(), "desc", NonConformanceReportSeverity.Minor, Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<FusionOS.BuildingBlocks.Application.Exceptions.ValidationException>();
    }

    [Fact]
    public async Task UpdateStatus_MovesReport()
    {
        var companyId = Guid.NewGuid();
        var ncr = NonConformanceReport.Create(companyId, null, "desc", NonConformanceReportSeverity.Minor, Guid.NewGuid());
        var repository = Substitute.For<INonConformanceReportRepository>();
        repository.GetByIdAsync(companyId, ncr.Id, Arg.Any<CancellationToken>()).Returns(ncr);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateNonConformanceReportStatusCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(
            new UpdateNonConformanceReportStatusCommand(companyId, ncr.Id, NonConformanceReportStatus.UnderReview),
            CancellationToken.None);

        result.Status.Should().Be("UnderReview");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateStatus_WhenMissing_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<INonConformanceReportRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((NonConformanceReport?)null);
        var handler = new UpdateNonConformanceReportStatusCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(
            new UpdateNonConformanceReportStatusCommand(companyId, Guid.NewGuid(), NonConformanceReportStatus.UnderReview),
            CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
