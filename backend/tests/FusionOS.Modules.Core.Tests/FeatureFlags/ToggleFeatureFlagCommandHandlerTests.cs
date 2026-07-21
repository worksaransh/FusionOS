using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.FeatureFlags.Commands.ToggleFeatureFlag;
using FusionOS.Modules.Core.Application.FeatureFlags.Contracts;
using FusionOS.Modules.Core.Domain.FeatureFlags;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.FeatureFlags;

public class ToggleFeatureFlagCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenFlagExists_FlipsIsEnabled()
    {
        var companyId = Guid.NewGuid();
        var flag = FeatureFlag.Create(companyId, "my-flag", "Name", null);
        flag.IsEnabled.Should().BeTrue();
        var repository = Substitute.For<IFeatureFlagRepository>();
        repository.GetByIdAsync(companyId, flag.Id, Arg.Any<CancellationToken>()).Returns(flag);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ToggleFeatureFlagCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new ToggleFeatureFlagCommand(companyId, flag.Id), CancellationToken.None);

        result.IsEnabled.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenFlagNotFound_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var featureFlagId = Guid.NewGuid();
        var repository = Substitute.For<IFeatureFlagRepository>();
        repository.GetByIdAsync(companyId, featureFlagId, Arg.Any<CancellationToken>()).Returns((FeatureFlag?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ToggleFeatureFlagCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new ToggleFeatureFlagCommand(companyId, featureFlagId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
