using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Reports.Queries.GetPoStatusSummaryReport;
using FusionOS.Modules.Procurement.Domain.PurchaseOrders;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.Reports;

/// <summary>Covers GetPoStatusSummaryReportQuery (Phase M6, 2026-07-15) — every status is represented even at zero.</summary>
public class GetPoStatusSummaryReportQueryHandlerTests
{
    [Fact]
    public async Task Handle_IncludesEveryStatusEvenWhenTheRepositoryOmitsZeroCounts()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IPurchaseOrderRepository>();
        repository.CountByStatusAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<(PurchaseOrderStatus, int)> { (PurchaseOrderStatus.Draft, 3) });
        var handler = new GetPoStatusSummaryReportQueryHandler(repository);

        var result = await handler.Handle(new GetPoStatusSummaryReportQuery(companyId), CancellationToken.None);

        result.Lines.Should().HaveCount(4);
        result.Lines.Should().Contain(l => l.Status == "Draft" && l.Count == 3);
        result.Lines.Should().Contain(l => l.Status == "Approved" && l.Count == 0);
        result.Lines.Should().Contain(l => l.Status == "PartiallyReceived" && l.Count == 0);
        result.Lines.Should().Contain(l => l.Status == "FullyReceived" && l.Count == 0);
    }

    [Fact]
    public async Task Handle_TotalCountIsTheSumAcrossEveryStatus()
    {
        var companyId = Guid.NewGuid();
        var repository = Substitute.For<IPurchaseOrderRepository>();
        repository.CountByStatusAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<(PurchaseOrderStatus, int)>
            {
                (PurchaseOrderStatus.Draft, 2),
                (PurchaseOrderStatus.Approved, 1),
                (PurchaseOrderStatus.FullyReceived, 4),
            });
        var handler = new GetPoStatusSummaryReportQueryHandler(repository);

        var result = await handler.Handle(new GetPoStatusSummaryReportQuery(companyId), CancellationToken.None);

        result.TotalCount.Should().Be(7);
    }
}
