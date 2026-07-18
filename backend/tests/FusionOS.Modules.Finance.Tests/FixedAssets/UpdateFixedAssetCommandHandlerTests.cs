using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using FusionOS.Modules.Finance.Application.FixedAssets.Commands.UpdateFixedAsset;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using FusionOS.Modules.Finance.Domain.CostCenters;
using FusionOS.Modules.Finance.Domain.FixedAssets;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.FixedAssets;

public class UpdateFixedAssetCommandHandlerTests
{
    private static DateTimeOffset AcquisitionDate => new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WhenFixedAssetExists_UpdatesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var fixedAsset = FixedAsset.Create(companyId, "FA-100", "Old Name", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, 60);
        var repository = Substitute.For<IFixedAssetRepository>();
        repository.GetByIdAsync(companyId, fixedAsset.Id, Arg.Any<CancellationToken>()).Returns(fixedAsset);
        var costCenterRepository = Substitute.For<ICostCenterRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new UpdateFixedAssetCommandHandler(repository, costCenterRepository, unitOfWork);
        var command = new UpdateFixedAssetCommand(companyId, fixedAsset.Id, "New Name", null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("New Name");
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenFixedAssetDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var fixedAssetId = Guid.NewGuid();
        var repository = Substitute.For<IFixedAssetRepository>();
        repository.GetByIdAsync(companyId, fixedAssetId, Arg.Any<CancellationToken>()).Returns((FixedAsset?)null);
        var handler = new UpdateFixedAssetCommandHandler(repository, Substitute.For<ICostCenterRepository>(), Substitute.For<IUnitOfWork>());
        var command = new UpdateFixedAssetCommand(companyId, fixedAssetId, "Name", null);

        var act = () => handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenCostCenterDoesNotExist_Throws()
    {
        var companyId = Guid.NewGuid();
        var costCenterId = Guid.NewGuid();
        var fixedAsset = FixedAsset.Create(companyId, "FA-100", "Van", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, 60);
        var repository = Substitute.For<IFixedAssetRepository>();
        repository.GetByIdAsync(companyId, fixedAsset.Id, Arg.Any<CancellationToken>()).Returns(fixedAsset);
        var costCenterRepository = Substitute.For<ICostCenterRepository>();
        costCenterRepository.GetByIdAsync(companyId, costCenterId, Arg.Any<CancellationToken>()).Returns((CostCenter?)null);
        var handler = new UpdateFixedAssetCommandHandler(repository, costCenterRepository, Substitute.For<IUnitOfWork>());
        var command = new UpdateFixedAssetCommand(companyId, fixedAsset.Id, "Van", costCenterId);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
