using FluentAssertions;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using FusionOS.Modules.Finance.Application.FixedAssets.Queries.GetDepreciationSchedule;
using FusionOS.Modules.Finance.Domain.FixedAssets;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.FixedAssets;

public class GetDepreciationScheduleQueryHandlerTests
{
    private static DateTimeOffset AcquisitionDate => new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);

    // AcquisitionCost 24000, SalvageValue 4000, UsefulLifeMonths 60 -> monthly depreciation = 20000 / 60 = 333.33...

    [Fact]
    public async Task Handle_MidLife_ReturnsProRatedAccumulatedDepreciationAndBookValue()
    {
        var companyId = Guid.NewGuid();
        var fixedAsset = FixedAsset.Create(companyId, "FA-100", "Van", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, 60);
        var repository = Substitute.For<IFixedAssetRepository>();
        repository.GetByIdAsync(companyId, fixedAsset.Id, Arg.Any<CancellationToken>()).Returns(fixedAsset);
        var handler = new GetDepreciationScheduleQueryHandler(repository);
        // Exactly 24 whole months after acquisition.
        var asOfDate = AcquisitionDate.AddMonths(24);

        var result = await handler.Handle(new GetDepreciationScheduleQuery(companyId, fixedAsset.Id, asOfDate), CancellationToken.None);

        var expectedMonthly = (24000m - 4000m) / 60;
        result.MonthlyDepreciationAmount.Should().Be(expectedMonthly);
        result.MonthsElapsed.Should().Be(24);
        result.AccumulatedDepreciation.Should().Be(expectedMonthly * 24);
        result.BookValue.Should().Be(24000m - (expectedMonthly * 24));
    }

    [Fact]
    public async Task Handle_BeforeAcquisitionDate_ReturnsZeroMonthsElapsedAndFullBookValue()
    {
        var companyId = Guid.NewGuid();
        var fixedAsset = FixedAsset.Create(companyId, "FA-100", "Van", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, 60);
        var repository = Substitute.For<IFixedAssetRepository>();
        repository.GetByIdAsync(companyId, fixedAsset.Id, Arg.Any<CancellationToken>()).Returns(fixedAsset);
        var handler = new GetDepreciationScheduleQueryHandler(repository);
        var asOfDate = AcquisitionDate.AddDays(-30);

        var result = await handler.Handle(new GetDepreciationScheduleQuery(companyId, fixedAsset.Id, asOfDate), CancellationToken.None);

        result.MonthsElapsed.Should().Be(0);
        result.AccumulatedDepreciation.Should().Be(0m);
        result.BookValue.Should().Be(24000m);
    }

    [Fact]
    public async Task Handle_PastUsefulLife_CapsMonthsElapsedAtUsefulLifeMonths()
    {
        var companyId = Guid.NewGuid();
        var fixedAsset = FixedAsset.Create(companyId, "FA-100", "Van", Guid.NewGuid(), null, null, AcquisitionDate, 24000m, 4000m, 60);
        var repository = Substitute.For<IFixedAssetRepository>();
        repository.GetByIdAsync(companyId, fixedAsset.Id, Arg.Any<CancellationToken>()).Returns(fixedAsset);
        var handler = new GetDepreciationScheduleQueryHandler(repository);
        // Well past the 60-month useful life.
        var asOfDate = AcquisitionDate.AddMonths(120);

        var result = await handler.Handle(new GetDepreciationScheduleQuery(companyId, fixedAsset.Id, asOfDate), CancellationToken.None);

        result.MonthsElapsed.Should().Be(60);
        result.AccumulatedDepreciation.Should().Be(24000m - 4000m);
        result.BookValue.Should().Be(4000m);
    }

    [Fact]
    public async Task Handle_WhenFixedAssetDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var fixedAssetId = Guid.NewGuid();
        var repository = Substitute.For<IFixedAssetRepository>();
        repository.GetByIdAsync(companyId, fixedAssetId, Arg.Any<CancellationToken>()).Returns((FixedAsset?)null);
        var handler = new GetDepreciationScheduleQueryHandler(repository);

        var act = () => handler.Handle(new GetDepreciationScheduleQuery(companyId, fixedAssetId, AcquisitionDate.AddYears(1)), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
