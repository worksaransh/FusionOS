using FluentAssertions;
using FusionOS.Modules.Crm.Domain.Activities;
using FusionOS.Modules.Crm.Domain.Activities.Events;
using Xunit;

namespace FusionOS.Modules.Crm.Tests.Activities;

public class ActivityTests
{
    private static readonly Guid Company = Guid.NewGuid();

    [Fact]
    public void Log_WithValidData_RaisesActivityLogged()
    {
        var entityId = Guid.NewGuid();

        var activity = Activity.Log(Company, "Lead", entityId, ActivityType.Call, " Discussed pricing. ");

        activity.EntityType.Should().Be("Lead");
        activity.EntityId.Should().Be(entityId);
        activity.Type.Should().Be(ActivityType.Call);
        activity.Notes.Should().Be("Discussed pricing.");
        activity.DomainEvents.Should().ContainSingle(e => e is ActivityLogged);
    }

    [Fact]
    public void Log_WithBlankNotes_Throws()
    {
        var act = () => Activity.Log(Company, "Lead", Guid.NewGuid(), ActivityType.Note, "   ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Log_WithEmptyEntityId_Throws()
    {
        var act = () => Activity.Log(Company, "Lead", Guid.Empty, ActivityType.Note, "Note");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Log_WithBlankEntityType_Throws()
    {
        var act = () => Activity.Log(Company, "  ", Guid.NewGuid(), ActivityType.Note, "Note");

        act.Should().Throw<ArgumentException>();
    }
}
