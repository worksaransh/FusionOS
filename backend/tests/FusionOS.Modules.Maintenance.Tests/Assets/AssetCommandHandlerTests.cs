using FluentAssertions;
using FusionOS.Modules.Maintenance.Application.Assets.Commands.CreateAsset;
using FusionOS.Modules.Maintenance.Application.Assets.Commands.DeactivateAsset;
using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using FusionOS.Modules.Maintenance.Domain.Assets;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Maintenance.Tests.Assets;

public class AssetCommandHandlerTests
{
    [Fact]
    public async Task CreateAsset_PersistsActiveAsset()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IAssetRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateAssetCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new CreateAssetCommand(companyId, "GEN-01", "Generator 1", "Bay 3"), CancellationToken.None);

        result.Code.Should().Be("GEN-01");
        result.IsActive.Should().BeTrue();
        await repository.Received(1).AddAsync(Arg.Any<Asset>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateAsset_SetsInactive()
    {
        var companyId = Guid.NewGuid();
        var asset = Asset.Create(companyId, "GEN-01", "Generator 1", null);
        var repository = Substitute.For<IAssetRepository>();
        repository.GetByIdAsync(companyId, asset.Id, Arg.Any<CancellationToken>()).Returns(asset);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateAssetCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateAssetCommand(companyId, asset.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeactivateAsset_WhenMissing_ThrowsKeyNotFound()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IAssetRepository>();
        repository.GetByIdAsync(companyId, Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Asset?)null);
        var handler = new DeactivateAssetCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new DeactivateAssetCommand(companyId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
