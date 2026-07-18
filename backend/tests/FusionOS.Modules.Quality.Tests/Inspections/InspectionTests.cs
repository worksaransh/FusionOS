using FluentAssertions;
using FusionOS.Modules.Quality.Domain.Inspections;
using FusionOS.Modules.Quality.Domain.Inspections.Events;
using Xunit;

namespace FusionOS.Modules.Quality.Tests.Inspections;

public class InspectionTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid Reference = Guid.NewGuid();

    private static Inspection New() =>
        Inspection.Create(Company, InspectionType.IncomingGoods, Reference, new[] { "Dimensional tolerance", "Surface finish" });

    [Fact]
    public void Create_Pending_WithItems_RaisesCreatedEvent()
    {
        var inspection = New();

        inspection.Status.Should().Be(InspectionStatus.Pending);
        inspection.Items.Should().HaveCount(2);
        inspection.DomainEvents.Should().ContainSingle(e => e is InspectionCreated);
    }

    [Fact]
    public void Create_DuplicateCharacteristic_Throws()
    {
        var act = () => Inspection.Create(Company, InspectionType.Production, Reference, new[] { "Weight", "weight" });

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordResults_AllPass_ResolvesPassed_AndRaisesCompleted()
    {
        var inspection = New();

        inspection.RecordResults(new[]
        {
            new InspectionResultInput("Dimensional tolerance", true, null),
            new InspectionResultInput("Surface finish", true, "clean"),
        });

        inspection.Status.Should().Be(InspectionStatus.Passed);
        var evt = inspection.DomainEvents.OfType<InspectionCompleted>().Single();
        evt.Passed.Should().BeTrue();
    }

    [Fact]
    public void RecordResults_AnyFail_ResolvesFailed()
    {
        var inspection = New();

        inspection.RecordResults(new[]
        {
            new InspectionResultInput("Dimensional tolerance", true, null),
            new InspectionResultInput("Surface finish", false, "scratches"),
        });

        inspection.Status.Should().Be(InspectionStatus.Failed);
        inspection.DomainEvents.OfType<InspectionCompleted>().Single().Passed.Should().BeFalse();
    }

    [Fact]
    public void RecordResults_MissingCharacteristic_Throws()
    {
        var inspection = New();

        var act = () => inspection.RecordResults(new[] { new InspectionResultInput("Dimensional tolerance", true, null) });

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RecordResults_Twice_Throws()
    {
        var inspection = New();
        inspection.RecordResults(new[]
        {
            new InspectionResultInput("Dimensional tolerance", true, null),
            new InspectionResultInput("Surface finish", true, null),
        });

        var act = () => inspection.RecordResults(new[]
        {
            new InspectionResultInput("Dimensional tolerance", true, null),
            new InspectionResultInput("Surface finish", true, null),
        });

        act.Should().Throw<InvalidOperationException>();
    }
}
