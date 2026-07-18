using FluentAssertions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.FixedAssets.Commands.DisposeFixedAsset;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using FusionOS.Modules.Finance.Domain.FixedAssets;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.FixedAssets;

public class DisposeFixedAssetCommandHandlerTests
{
    private static DateTimeOffset AcquisitionDate => new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WhenFixedAssetExists_DisposesAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var fixedAsset = FixedAsset.Create(companyId, "FA-100", "Van", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, 60);
        var repository = Substitute.For<IFixedAssetRepository>();
        repository.GetByIdAsync(companyId, fixedAsset.Id, Arg.Any<CancellationToken>()).Returns(fixedAsset);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new DisposeFixedAssetCommandHandler(repository, unitOfWork);
        var disposedDate = AcquisitionDate.AddYears(2);

        var result = await handler.Handle(new DisposeFixedAssetCommand(companyId, fixedAsset.Id, disposedDate), CancellationToken.None);

        result.IsDisposed.Should().BeTrue();
        result.DisposedDate.Should().Be(disposedDate);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenFixedAssetDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var fixedAssetId = Guid.NewGuid();
        var repository = Substitute.For<IFixedAssetRepository>();
        repository.GetByIdAsync(companyId, fixedAssetId, Arg.Any<CancellationToken>()).Returns((FixedAsset?)null);
        var handler = new DisposeFixedAssetCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = () => handler.Handle(new DisposeFixedAssetCommand(companyId, fixedAssetId, AcquisitionDate.AddYears(1)), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Handle_WhenDisposedDateBeforeAcquisitionDate_Throws()
    {
        var companyId = Guid.NewGuid();
        var fixedAsset = FixedAsset.Create(companyId, "FA-100", "Van", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, 60);
        var repository = Substitute.For<IFixedAssetRepository>();
        repository.GetByIdAsync(companyId, fixedAsset.Id, Arg.Any<CancellationToken>()).Returns(fixedAsset);
        var handler = new DisposeFixedAssetCommandHandler(repository, Substitute.For<IUnitOfWork>());

        var act = async () => await handler.Handle(new DisposeFixedAssetCommand(companyId, fixedAsset.Id, AcquisitionDate.AddDays(-1)), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
