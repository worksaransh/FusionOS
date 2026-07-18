using FusionOS.Modules.Finance.Application.Receivables.Contracts;
using FusionOS.Modules.Finance.Application.Reports.Queries.GetArAgingReport;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Reports;

/// <summary>
/// Covers GetArAgingReportQuery (Phase M6, 2026-07-15) — the bucketing logic
/// (0-30 / 31-60 / 61-90 / 90+ by days since ChargeDate) and the total roll-up,
/// given a repository that already did the "net balance per invoice" work.
/// </summary>
public class GetArAgingReportQueryHandlerTests
{
    [Theory]
    [InlineData(0, "0-30")]
    [InlineData(30, "0-30")]
    [InlineData(31, "31-60")]
    [InlineData(60, "31-60")]
    [InlineData(61, "61-90")]
    [InlineData(90, "61-90")]
    [InlineData(91, "90+")]
    public async Task Handle_BucketsEachInvoiceByDaysSinceItsChargeDate(int daysAgo, string expectedBucket)
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var invoiceId = Guid.NewGuid();
        var repository = Substitute.For<IArLedgerRepository>();
        repository.GetOutstandingInvoiceBalancesAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid, Guid, decimal, DateTimeOffset)>
            {
                (customerId, invoiceId, 500m, DateTimeOffset.UtcNow.AddDays(-daysAgo)),
            });
        var handler = new GetArAgingReportQueryHandler(repository);

        var result = await handler.Handle(new GetArAgingReportQuery(companyId), CancellationToken.None);

        result.Lines.Should().ContainSingle();
        result.Lines[0].Bucket.Should().Be(expectedBucket);
        result.GrandTotal.Should().Be(500m);
    }

    [Fact]
    public async Task Handle_WithNoOutstandingBalances_ReturnsAllZeroTotals()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IArLedgerRepository>();
        repository.GetOutstandingInvoiceBalancesAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid, Guid, decimal, DateTimeOffset)>());
        var handler = new GetArAgingReportQueryHandler(repository);

        var result = await handler.Handle(new GetArAgingReportQuery(companyId), CancellationToken.None);

        result.Lines.Should().BeEmpty();
        result.GrandTotal.Should().Be(0m);
        result.Bucket0To30Total.Should().Be(0m);
        result.Bucket90PlusTotal.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_SumsEachBucketIndependently()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IArLedgerRepository>();
        repository.GetOutstandingInvoiceBalancesAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid, Guid, decimal, DateTimeOffset)>
            {
                (Guid.NewGuid(), Guid.NewGuid(), 100m, DateTimeOffset.UtcNow.AddDays(-5)),   // 0-30
                (Guid.NewGuid(), Guid.NewGuid(), 200m, DateTimeOffset.UtcNow.AddDays(-45)),  // 31-60
                (Guid.NewGuid(), Guid.NewGuid(), 300m, DateTimeOffset.UtcNow.AddDays(-120)), // 90+
            });
        var handler = new GetArAgingReportQueryHandler(repository);

        var result = await handler.Handle(new GetArAgingReportQuery(companyId), CancellationToken.None);

        result.Bucket0To30Total.Should().Be(100m);
        result.Bucket31To60Total.Should().Be(200m);
        result.Bucket61To90Total.Should().Be(0m);
        result.Bucket90PlusTotal.Should().Be(300m);
        result.GrandTotal.Should().Be(600m);
    }
}
