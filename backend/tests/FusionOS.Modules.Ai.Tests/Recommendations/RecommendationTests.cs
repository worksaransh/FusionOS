using FluentAssertions;
using FusionOS.Modules.Ai.Domain.Recommendations;
using FusionOS.Modules.Ai.Domain.Recommendations.Events;
using Xunit;

namespace FusionOS.Modules.Ai.Tests.Recommendations;

public class RecommendationTests
{
    private static readonly Guid Company = Guid.NewGuid();
    private static readonly Guid Reference = Guid.NewGuid();

    private static Recommendation New() =>
        Recommendation.Create(Company, "ReorderSuggestion", Reference, "Reorder 200 units based on 90-day demand trend.", "v1.0.0");

    [Fact]
    public void Create_Pending_RaisesCreatedEvent()
    {
        var recommendation = New();

        recommendation.Status.Should().Be(RecommendationStatus.Pending);
        recommendation.DomainEvents.Should().ContainSingle(e => e is RecommendationCreated);
    }

    [Fact]
    public void Create_WithBlankSummary_Throws()
    {
        var act = () => Recommendation.Create(Company, "ReorderSuggestion", Reference, "  ", "v1.0.0");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Accept_FromPending_TransitionsAndRaisesAccepted()
    {
        var recommendation = New();

        recommendation.Accept();

        recommendation.Status.Should().Be(RecommendationStatus.Accepted);
        recommendation.DecidedAt.Should().NotBeNull();
        recommendation.DomainEvents.Should().ContainSingle(e => e is RecommendationAccepted);
    }

    [Fact]
    public void Accept_WhenNotPending_Throws()
    {
        var recommendation = New();
        recommendation.Accept();

        var act = () => recommendation.Accept();

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Dismiss_FromPending_Transitions()
    {
        var recommendation = New();

        recommendation.Dismiss();

        recommendation.Status.Should().Be(RecommendationStatus.Dismissed);
        recommendation.DecidedAt.Should().NotBeNull();
    }

    [Fact]
    public void Dismiss_WhenNotPending_Throws()
    {
        var recommendation = New();
        recommendation.Dismiss();

        var act = () => recommendation.Dismiss();

        act.Should().Throw<InvalidOperationException>();
    }
}
