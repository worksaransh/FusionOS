using FusionOS.Modules.Sales.Application.Commissions.Contracts;
using FusionOS.Modules.Sales.Application.Commissions.Queries.GetSalesCommissionSummaryReport;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;
using FusionOS.Modules.Sales.Domain.Commissions;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Commissions;

public class GetSalesCommissionSummaryReportQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithRateSet_ComputesCommissionAmount()
    {
        var companyId = Guid.NewGuid();
        var salesPersonId = Guid.NewGuid();
        var invoiceRepository = Substitute.For<IInvoiceRepository>();
        var rateRepository = Substitute.For<ISalesCommissionRateRepository>();
        invoiceRepository.GetIssuedInvoiceTotalsBySalesPersonAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid SalesPersonId, decimal TotalInvoicedRevenue)> { (salesPersonId, 1000m) });
        rateRepository.GetByUserIdAsync(companyId, salesPersonId, Arg.Any<CancellationToken>())
            .Returns(SalesCommissionRate.Create(companyId, salesPersonId, 5m));
        var handler = new GetSalesCommissionSummaryReportQueryHandler(invoiceRepository, rateRepository);

        var result = await handler.Handle(new GetSalesCommissionSummaryReportQuery(companyId), CancellationToken.None);

        result.Should().ContainSingle();
        result[0].TotalInvoicedRevenue.Should().Be(1000m);
        result[0].RatePercentage.Should().Be(5m);
        result[0].CommissionAmount.Should().Be(50m);
    }

    [Fact]
    public async Task Handle_WithNoRateSet_DefaultsToZeroPercent()
    {
        var companyId = Guid.NewGuid();
        var salesPersonId = Guid.NewGuid();
        var invoiceRepository = Substitute.For<IInvoiceRepository>();
        var rateRepository = Substitute.For<ISalesCommissionRateRepository>();
        invoiceRepository.GetIssuedInvoiceTotalsBySalesPersonAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<(Guid SalesPersonId, decimal TotalInvoicedRevenue)> { (salesPersonId, 1000m) });
        rateRepository.GetByUserIdAsync(companyId, salesPersonId, Arg.Any<CancellationToken>())
            .Returns((SalesCommissionRate?)null);
        var handler = new GetSalesCommissionSummaryReportQueryHandler(invoiceRepository, rateRepository);

        var result = await handler.Handle(new GetSalesCommissionSummaryReportQuery(companyId), CancellationToken.None);

        result[0].RatePercentage.Should().Be(0m);
        result[0].CommissionAmount.Should().Be(0m);
    }
}
