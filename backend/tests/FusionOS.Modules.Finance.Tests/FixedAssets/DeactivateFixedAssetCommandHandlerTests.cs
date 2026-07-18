using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.FixedAssets.Commands.DeactivateFixedAsset;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using FusionOS.Modules.Finance.Domain.FixedAssets;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.FixedAssets;

public class DeactivateFixedAssetCommandHandlerTests
{
    private static DateTimeOffset AcquisitionDate => new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WhenFixedAssetExists_DeactivatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var fixedAsset = FixedAsset.Create(companyId, "FA-100", "Van", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, 60);
        var repository = Substitute.For<IFixedAssetRepository>();
        repository.GetByIdAsync(companyId, fixedAsset.Id, Arg.Any<CancellationToken>()).Returns(fixedAsset);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DeactivateFixedAssetCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new DeactivateFixedAssetCommand(companyId, fixedAsset.Id), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenFixedAssetDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var fixedAssetId = Guid.NewGuid();
        var repository = Substitute.For<IFixedAssetRepository>();
        repository.GetByIdAsync(companyId, fixedAssetId, Arg.Any<CancellationToken>()).Returns((FixedAsset?)null);
        var handler = new DeactivateFixedAssetCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new DeactivateFixedAssetCommand(companyId, fixedAssetId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
