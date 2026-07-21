using FusionOS.Modules.Core.Application.FeatureFlags.Contracts;
using FusionOS.Modules.Core.Application.FeatureFlags.Queries.IsFeatureEnabled;
using FusionOS.Modules.Core.Domain.FeatureFlags;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.FeatureFlags;

public class IsFeatureEnabledQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenFlagUnknown_ReturnsFalse_ButDoesNotThrow()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IFeatureFlagRepository>();
        repository.GetByKeyAsync(companyId, "unknown-flag", Arg.Any<CancellationToken>()).Returns((FeatureFlag?)null);
        var handler = new IsFeatureEnabledQueryHandler(repository);

        var result = await handler.Handle(new IsFeatureEnabledQuery(companyId, "unknown-flag"), CancellationToken.None);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenFlagEnabledAtFullRollout_ReturnsTrue()
    {
        var companyId = Guid.NewGuid();
        var flag = FeatureFlag.Create(companyId, "my-flag", "Name", null, rolloutPercentage: 100);
        var repository = Substitute.For<IFeatureFlagRepository>();
        repository.GetByKeyAsync(companyId, "my-flag", Arg.Any<CancellationToken>()).Returns(flag);
        var handler = new IsFeatureEnabledQueryHandler(repository);

        var result = await handler.Handle(new IsFeatureEnabledQuery(companyId, "my-flag", "user-1"), CancellationToken.None);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WhenFlagDisabled_ReturnsFalse_EvenAtFullRollout()
    {
        var companyId = Guid.NewGuid();
        var flag = FeatureFlag.Create(companyId, "my-flag", "Name", null, rolloutPercentage: 100);
        flag.Toggle();
        var repository = Substitute.For<IFeatureFlagRepository>();
        repository.GetByKeyAsync(companyId, "my-flag", Arg.Any<CancellationToken>()).Returns(flag);
        var handler = new IsFeatureEnabledQueryHandler(repository);

        var result = await handler.Handle(new IsFeatureEnabledQuery(companyId, "my-flag", "user-1"), CancellationToken.None);

        result.Should().BeFalse();
    }
}
