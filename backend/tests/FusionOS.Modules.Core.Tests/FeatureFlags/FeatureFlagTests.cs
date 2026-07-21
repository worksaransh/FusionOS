using FusionOS.Modules.Core.Domain.FeatureFlags;
using FluentAssertions;
using Xunit;

namespace FusionOS.Modules.Core.Tests.FeatureFlags;

public class FeatureFlagTests
{
    [Fact]
    public void Create_WithValidData_RaisesFeatureFlagCreatedEvent()
    {
        var companyId = Guid.NewGuid();

        var flag = FeatureFlag.Create(companyId, "new-dashboard-widget", "New Dashboard Widget", "Shows the new widget.");

        flag.Key.Should().Be("new-dashboard-widget");
        flag.Name.Should().Be("New Dashboard Widget");
        flag.IsEnabled.Should().BeTrue();
        flag.RolloutPercentage.Should().Be(100);
        flag.DomainEvents.Should().ContainSingle(e => e is Events.FeatureFlagCreated);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Create_WithoutKey_Throws(string invalidKey)
    {
        var act = () => FeatureFlag.Create(Guid.NewGuid(), invalidKey, "Name", null);

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Create_WithOutOfRangePercentage_Throws(int invalidPercentage)
    {
        var act = () => FeatureFlag.Create(Guid.NewGuid(), "flag", "Name", null, invalidPercentage);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void UpdateDetails_ChangesNameDescriptionAndPercentage_ButNotKey()
    {
        var flag = FeatureFlag.Create(Guid.NewGuid(), "my-flag", "Old Name", "Old description");

        flag.UpdateDetails("New Name", "New description", 50);

        flag.Key.Should().Be("my-flag");
        flag.Name.Should().Be("New Name");
        flag.Description.Should().Be("New description");
        flag.RolloutPercentage.Should().Be(50);
    }

    [Fact]
    public void Toggle_FlipsIsEnabled()
    {
        var flag = FeatureFlag.Create(Guid.NewGuid(), "my-flag", "Name", null);
        flag.IsEnabled.Should().BeTrue();

        flag.Toggle();
        flag.IsEnabled.Should().BeFalse();

        flag.Toggle();
        flag.IsEnabled.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_WhenDisabled_IsAlwaysFalse_RegardlessOfPercentage()
    {
        var flag = FeatureFlag.Create(Guid.NewGuid(), "my-flag", "Name", null, rolloutPercentage: 100);
        flag.Toggle(); // now disabled

        flag.Evaluate(null).Should().BeFalse();
        flag.Evaluate("user-1").Should().BeFalse();
    }

    [Fact]
    public void Evaluate_WhenEnabledWithNoEvaluationId_ReturnsIsEnabled()
    {
        var flag = FeatureFlag.Create(Guid.NewGuid(), "my-flag", "Name", null, rolloutPercentage: 1);

        flag.Evaluate(null).Should().BeTrue();
    }

    [Fact]
    public void Evaluate_WithZeroPercent_IsAlwaysFalse_EvenWhenEnabled()
    {
        var flag = FeatureFlag.Create(Guid.NewGuid(), "my-flag", "Name", null, rolloutPercentage: 0);

        flag.Evaluate("any-user").Should().BeFalse();
    }

    [Fact]
    public void Evaluate_WithHundredPercent_IsAlwaysTrue()
    {
        var flag = FeatureFlag.Create(Guid.NewGuid(), "my-flag", "Name", null, rolloutPercentage: 100);

        flag.Evaluate("any-user-1").Should().BeTrue();
        flag.Evaluate("any-user-2").Should().BeTrue();
    }

    [Fact]
    public void Evaluate_SameKeyAndEvaluationId_IsDeterministicAcrossCalls()
    {
        var flag = FeatureFlag.Create(Guid.NewGuid(), "my-flag", "Name", null, rolloutPercentage: 50);

        var first = flag.Evaluate("user-42");
        var second = flag.Evaluate("user-42");
        var third = flag.Evaluate("user-42");

        second.Should().Be(first);
        third.Should().Be(first);
    }

    [Fact]
    public void Evaluate_DifferentEvaluationIds_CanYieldDifferentBuckets()
    {
        // Not every id is guaranteed to land in a different bucket, but across many
        // distinct ids at 50% we should see both true and false at least once —
        // proves the hash isn't a constant/degenerate function.
        var flag = FeatureFlag.Create(Guid.NewGuid(), "my-flag", "Name", null, rolloutPercentage: 50);

        var results = Enumerable.Range(0, 200)
            .Select(i => flag.Evaluate($"user-{i}"))
            .ToList();

        results.Should().Contain(true);
        results.Should().Contain(false);
    }
}
