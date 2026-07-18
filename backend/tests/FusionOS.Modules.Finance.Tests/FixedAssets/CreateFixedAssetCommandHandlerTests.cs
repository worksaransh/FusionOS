using FluentAssertions;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using FusionOS.Modules.Finance.Application.FixedAssets.Commands.CreateFixedAsset;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using FusionOS.Modules.Finance.Domain.CostCenters;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.FixedAssets;

public class CreateFixedAssetCommandHandlerTests
{
    private static DateTimeOffset AcquisitionDate => new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private static (IFixedAssetRepository repo, IAccountRepository accountRepo, ICostCenterRepository costCenterRepo, IUnitOfWork uow) MakeSubstitutes(Guid companyId, Guid assetAccountId)
    {
        var repo = Substitute.For<IFixedAssetRepository>();
        var accountRepo = Substitute.For<IAccountRepository>();
        accountRepo.ExistsAsync(companyId, assetAccountId, Arg.Any<CancellationToken>()).Returns(true);
        var costCenterRepo = Substitute.For<ICostCenterRepository>();
        var uow = Substitute.For<IUnitOfWork>();
        return (repo, accountRepo, costCenterRepo, uow);
    }

    [Fact]
    public async Task Handle_WithValidData_PersistsFixedAsset()
    {
        var companyId = Guid.NewGuid();
        var assetAccountId = Guid.NewGuid();
        var (repo, accountRepo, costCenterRepo, uow) = MakeSubstitutes(companyId, assetAccountId);
        var handler = new CreateFixedAssetCommandHandler(repo, accountRepo, costCenterRepo, uow);
        var command = new CreateFixedAssetCommand(companyId, "FA-100", "Delivery Van #3", assetAccountId, null, null, AcquisitionDate, 24000m, 4000m, 60);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Code.Should().Be("FA-100");
        result.Name.Should().Be("Delivery Van #3");
        result.AcquisitionCost.Should().Be(24000m);
        await repo.Received(1).AddAsync(Arg.Any<FusionOS.Modules.Finance.Domain.FixedAssets.FixedAsset>(), Arg.Any<CancellationToken>());
        await uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCodeAlreadyExists_Throws()
    {
        var companyId = Guid.NewGuid();
        var assetAccountId = Guid.NewGuid();
        var (repo, accountRepo, costCenterRepo, uow) = MakeSubstitutes(companyId, assetAccountId);
        repo.CodeExistsAsync(companyId, "FA-100", Arg.Any<CancellationToken>()).Returns(true);
        var handler = new CreateFixedAssetCommandHandler(repo, accountRepo, costCenterRepo, uow);
        var command = new CreateFixedAssetCommand(companyId, "FA-100", "Van", assetAccountId, null, null, AcquisitionDate, 24000m, 4000m, 60);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenAssetAccountDoesNotExist_Throws()
    {
        var companyId = Guid.NewGuid();
        var assetAccountId = Guid.NewGuid();
        var (repo, accountRepo, costCenterRepo, uow) = MakeSubstitutes(companyId, assetAccountId);
        accountRepo.ExistsAsync(companyId, assetAccountId, Arg.Any<CancellationToken>()).Returns(false);
        var handler = new CreateFixedAssetCommandHandler(repo, accountRepo, costCenterRepo, uow);
        var command = new CreateFixedAssetCommand(companyId, "FA-100", "Van", assetAccountId, null, null, AcquisitionDate, 24000m, 4000m, 60);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenAccumulatedDepreciationAccountDoesNotExist_Throws()
    {
        var companyId = Guid.NewGuid();
        var assetAccountId = Guid.NewGuid();
        var accumulatedDepreciationAccountId = Guid.NewGuid();
        var (repo, accountRepo, costCenterRepo, uow) = MakeSubstitutes(companyId, assetAccountId);
        accountRepo.ExistsAsync(companyId, accumulatedDepreciationAccountId, Arg.Any<CancellationToken>()).Returns(false);
        var handler = new CreateFixedAssetCommandHandler(repo, accountRepo, costCenterRepo, uow);
        var command = new CreateFixedAssetCommand(companyId, "FA-100", "Van", assetAccountId, accumulatedDepreciationAccountId, null, AcquisitionDate, 24000m, 4000m, 60);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_WhenCostCenterDoesNotExist_Throws()
    {
        var companyId = Guid.NewGuid();
        var assetAccountId = Guid.NewGuid();
        var costCenterId = Guid.NewGuid();
        var (repo, accountRepo, costCenterRepo, uow) = MakeSubstitutes(companyId, assetAccountId);
        costCenterRepo.GetByIdAsync(companyId, costCenterId, Arg.Any<CancellationToken>()).Returns((CostCenter?)null);
        var handler = new CreateFixedAssetCommandHandler(repo, accountRepo, costCenterRepo, uow);
        var command = new CreateFixedAssetCommand(companyId, "FA-100", "Van", assetAccountId, null, costCenterId, AcquisitionDate, 24000m, 4000m, 60);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }
}
