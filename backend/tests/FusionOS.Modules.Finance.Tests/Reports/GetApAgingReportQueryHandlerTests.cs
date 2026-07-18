using FusionOS.Modules.Finance.Application.Payables.Contracts;
using FusionOS.Modules.Finance.Application.Reports.Queries.GetApAgingReport;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Reports;

/// <summary>
/// Covers GetApAgingReportQuery (Phase 2 closeout, 2026-07-18) — mirrors
/// GetArAgingReportQueryHandlerTests' exact bucketing coverage (0-30 / 31-60 /
/// 61-90 / 90+ by days since OldestChargeDate) and the total roll-up, given a
/// repository that already did the "net balance per supplier" work.
/// </summary>
public class GetApAgingReportQueryHandlerTests
{
    [Theory]
    [InlineData(0, "0-30")]
    [InlineData(30, "0-30")]
    [InlineData(31, "31-60")]
    [InlineData(60, "31-60")]
    [InlineData(61, "61-90")]
    [InlineData(90, "61-90")]
    [InlineData(91, "90+")]
    public async Task Handle_BucketsEachSupplierByDaysSinceItsOldestChargeDate(int daysAgo, string expectedBucket)
    {
        var companyId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var repository = Substitute.For<IApLedgerRepository>();
        repository.GetOutstandingSupplierBalancesAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid, decimal, DateTimeOffset)>
            {
                (supplierId, 500m, DateTimeOffset.UtcNow.AddDays(-daysAgo)),
            });
        var handler = new GetApAgingReportQueryHandler(repository);

        var result = await handler.Handle(new GetApAgingReportQuery(companyId), CancellationToken.None);

        result.Lines.Should().ContainSingle();
        result.Lines[0].Bucket.Should().Be(expectedBucket);
        result.GrandTotal.Should().Be(500m);
    }

    [Fact]
    public async Task Handle_WithNoOutstandingBalances_ReturnsAllZeroTotals()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IApLedgerRepository>();
        repository.GetOutstandingSupplierBalancesAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid, decimal, DateTimeOffset)>());
        var handler = new GetApAgingReportQueryHandler(repository);

        var result = await handler.Handle(new GetApAgingReportQuery(companyId), CancellationToken.None);

        result.Lines.Should().BeEmpty();
        result.GrandTotal.Should().Be(0m);
        result.Bucket0To30Total.Should().Be(0m);
        result.Bucket90PlusTotal.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_SumsEachBucketIndependently()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IApLedgerRepository>();
        repository.GetOutstandingSupplierBalancesAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid, decimal, DateTimeOffset)>
            {
                (Guid.NewGuid(), 100m, DateTimeOffset.UtcNow.AddDays(-5)),   // 0-30
                (Guid.NewGuid(), 200m, DateTimeOffset.UtcNow.AddDays(-45)),  // 31-60
                (Guid.NewGuid(), 300m, DateTimeOffset.UtcNow.AddDays(-120)), // 90+
            });
        var handler = new GetApAgingReportQueryHandler(repository);

        var result = await handler.Handle(new GetApAgingReportQuery(companyId), CancellationToken.None);

        result.Bucket0To30Total.Should().Be(100m);
        result.Bucket31To60Total.Should().Be(200m);
        result.Bucket61To90Total.Should().Be(0m);
        result.Bucket90PlusTotal.Should().Be(300m);
        result.GrandTotal.Should().Be(600m);
    }
}
