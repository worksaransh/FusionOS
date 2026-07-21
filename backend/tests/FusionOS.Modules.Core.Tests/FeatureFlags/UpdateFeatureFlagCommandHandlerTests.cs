using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.FeatureFlags.Commands.UpdateFeatureFlag;
using FusionOS.Modules.Core.Application.FeatureFlags.Contracts;
using FusionOS.Modules.Core.Domain.FeatureFlags;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.FeatureFlags;

public class UpdateFeatureFlagCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenFlagExists_UpdatesDetailsAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var flag = FeatureFlag.Create(companyId, "my-flag", "Old Name", "Old description");
        var repository = Substitute.For<IFeatureFlagRepository>();
        repository.GetByIdAsync(companyId, flag.Id, Arg.Any<CancellationToken>()).Returns(flag);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateFeatureFlagCommandHandler(repository, unitOfWork);
        var command = new UpdateFeatureFlagCommand(companyId, flag.Id, "New Name", "New description", 25);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("New Name");
        result.Description.Should().Be("New description");
        result.RolloutPercentage.Should().Be(25);
        result.Key.Should().Be("my-flag");
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
        var handler = new UpdateFeatureFlagCommandHandler(repository, unitOfWork);
        var command = new UpdateFeatureFlagCommand(companyId, featureFlagId, "Name", null, 100);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
