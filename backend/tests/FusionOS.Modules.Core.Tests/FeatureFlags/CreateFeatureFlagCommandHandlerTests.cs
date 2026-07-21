using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.FeatureFlags.Commands.CreateFeatureFlag;
using FusionOS.Modules.Core.Application.FeatureFlags.Contracts;
using FusionOS.Modules.Core.Domain.FeatureFlags;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Core.Tests.FeatureFlags;

public class CreateFeatureFlagCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithUniqueKey_CreatesFlagAndReturnsDto()
    {
        var repository = Substitute.For<IFeatureFlagRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var companyId = Guid.NewGuid();
        repository.KeyExistsAsync(companyId, "new-dashboard-widget", Arg.Any<CancellationToken>()).Returns(false);
        var handler = new CreateFeatureFlagCommandHandler(repository, unitOfWork);
        var command = new CreateFeatureFlagCommand(companyId, "new-dashboard-widget", "New Dashboard Widget", "desc", 75);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Key.Should().Be("new-dashboard-widget");
        result.RolloutPercentage.Should().Be(75);
        result.IsEnabled.Should().BeTrue();
        await repository.Received(1).AddAsync(Arg.Any<FeatureFlag>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDuplicateKey_ThrowsValidationException()
    {
        var repository = Substitute.For<IFeatureFlagRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var companyId = Guid.NewGuid();
        repository.KeyExistsAsync(companyId, "existing-flag", Arg.Any<CancellationToken>()).Returns(true);
        var handler = new CreateFeatureFlagCommandHandler(repository, unitOfWork);
        var command = new CreateFeatureFlagCommand(companyId, "existing-flag", "Existing Flag", null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
        await repository.DidNotReceive().AddAsync(Arg.Any<FeatureFlag>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
