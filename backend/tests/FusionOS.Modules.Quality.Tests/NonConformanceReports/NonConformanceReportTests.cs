using FluentAssertions;
using FusionOS.Modules.Quality.Domain.NonConformanceReports;
using FusionOS.Modules.Quality.Domain.NonConformanceReports.Events;
using Xunit;

namespace FusionOS.Modules.Quality.Tests.NonConformanceReports;

public class NonConformanceReportTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid RaisedBy = Guid.NewGuid();

    private static NonConformanceReport New(Guid? inspectionId = null) =>
        NonConformanceReport.Create(Company, inspectionId, "Bracket out of tolerance", NonConformanceReportSeverity.Major, RaisedBy);

    [Fact]
    public void Create_Open_RaisesCreatedEvent()
    {
        var ncr = New();

        ncr.Status.Should().Be(NonConformanceReportStatus.Open);
        ncr.InspectionId.Should().BeNull();
        ncr.DomainEvents.Should().ContainSingle(e => e is NonConformanceReportCreated);
    }

    [Fact]
    public void Create_WithInspectionId_SetsIt()
    {
        var inspectionId = Guid.NewGuid();

        var ncr = New(inspectionId);

        ncr.InspectionId.Should().Be(inspectionId);
    }

    [Fact]
    public void Create_EmptyInspectionId_Throws()
    {
        var act = () => NonConformanceReport.Create(Company, Guid.Empty, "desc", NonConformanceReportSeverity.Minor, RaisedBy);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_BlankDescription_Throws()
    {
        var act = () => NonConformanceReport.Create(Company, null, "  ", NonConformanceReportSeverity.Minor, RaisedBy);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_EmptyRaisedBy_Throws()
    {
        var act = () => NonConformanceReport.Create(Company, null, "desc", NonConformanceReportSeverity.Minor, Guid.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void UpdateStatus_ToUnderReview_Moves()
    {
        var ncr = New();

        ncr.UpdateStatus(NonConformanceReportStatus.UnderReview);

        ncr.Status.Should().Be(NonConformanceReportStatus.UnderReview);
    }

    [Fact]
    public void UpdateStatus_ToClosed_SetsClosedAt_AndRaisesClosedEvent()
    {
        var ncr = New();

        ncr.UpdateStatus(NonConformanceReportStatus.Closed);

        ncr.Status.Should().Be(NonConformanceReportStatus.Closed);
        ncr.ClosedAt.Should().NotBeNull();
        ncr.DomainEvents.Should().ContainSingle(e => e is NonConformanceReportClosed);
    }

    [Fact]
    public void UpdateStatus_BackToOpen_Throws()
    {
        var ncr = New();
        ncr.UpdateStatus(NonConformanceReportStatus.UnderReview);

        var act = () => ncr.UpdateStatus(NonConformanceReportStatus.Open);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UpdateStatus_SameStatus_Throws()
    {
        var ncr = New();

        var act = () => ncr.UpdateStatus(NonConformanceReportStatus.Open);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UpdateStatus_AfterClosed_Throws()
    {
        var ncr = New();
        ncr.UpdateStatus(NonConformanceReportStatus.Closed);

        var act = () => ncr.UpdateStatus(NonConformanceReportStatus.UnderReview);

        act.Should().Throw<InvalidOperationException>();
    }
}
