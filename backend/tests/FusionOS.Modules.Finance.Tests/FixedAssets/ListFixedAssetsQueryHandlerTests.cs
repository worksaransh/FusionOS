using FluentAssertions;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using FusionOS.Modules.Finance.Application.FixedAssets.Queries.ListFixedAssets;
using FusionOS.Modules.Finance.Domain.FixedAssets;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.FixedAssets;

public class ListFixedAssetsQueryHandlerTests
{
    private static DateTimeOffset AcquisitionDate => new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_ReturnsPagedFixedAssetsForTheCompany()
    {
        var companyId = Guid.NewGuid();
        var fixedAssets = new[] { FixedAsset.Create(companyId, "FA-100", "Delivery Van #3", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, 60) };
        var repository = Substitute.For<IFixedAssetRepository>();
        repository.ListAsync(companyId, null, null, 1, 25, Arg.Any<CancellationToken>()).Returns(fixedAssets);
        repository.CountAsync(companyId, null, null, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListFixedAssetsQueryHandler(repository);

        var result = await handler.Handle(new ListFixedAssetsQuery(companyId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(a => a.Code == "FA-100");
    }

    [Fact]
    public async Task Handle_PassesIsDisposedAndIsActiveFiltersThrough()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IFixedAssetRepository>();
        repository.ListAsync(companyId, true, false, 1, 25, Arg.Any<CancellationToken>()).Returns(Array.Empty<FixedAsset>());
        repository.CountAsync(companyId, true, false, Arg.Any<CancellationToken>()).Returns(0);
        var handler = new ListFixedAssetsQueryHandler(repository);

        var result = await handler.Handle(new ListFixedAssetsQuery(companyId, true, false), CancellationToken.None);

        result.TotalCount.Should().Be(0);
        await repository.Received(1).ListAsync(companyId, true, false, 1, 25, Arg.Any<CancellationToken>());
    }
}
