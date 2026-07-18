using FluentAssertions;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using FusionOS.Modules.Finance.Application.FixedAssets.Queries.GetFixedAssetById;
using FusionOS.Modules.Finance.Domain.FixedAssets;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.FixedAssets;

public class GetFixedAssetByIdQueryHandlerTests
{
    private static DateTimeOffset AcquisitionDate => new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_WhenFixedAssetExists_ReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var fixedAsset = FixedAsset.Create(companyId, "FA-100", "Delivery Van #3", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, 60);
        var repository = Substitute.For<IFixedAssetRepository>();
        repository.GetByIdAsync(companyId, fixedAsset.Id, Arg.Any<CancellationToken>()).Returns(fixedAsset);
        var handler = new GetFixedAssetByIdQueryHandler(repository);

        var result = await handler.Handle(new GetFixedAssetByIdQuery(companyId, fixedAsset.Id), CancellationToken.None);

        result.Code.Should().Be("FA-100");
        result.Name.Should().Be("Delivery Van #3");
    }

    [Fact]
    public async Task Handle_WhenFixedAssetDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var fixedAssetId = Guid.NewGuid();
        var repository = Substitute.For<IFixedAssetRepository>();
        repository.GetByIdAsync(companyId, fixedAssetId, Arg.Any<CancellationToken>()).Returns((FixedAsset?)null);
        var handler = new GetFixedAssetByIdQueryHandler(repository);

        var act = () => handler.Handle(new GetFixedAssetByIdQuery(companyId, fixedAssetId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
