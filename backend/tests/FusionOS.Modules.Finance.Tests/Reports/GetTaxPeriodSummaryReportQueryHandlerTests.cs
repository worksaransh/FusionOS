using FluentAssertions;
using FusionOS.Modules.Finance.Application.Reports.Queries.GetTaxPeriodSummaryReport;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Contracts;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;
using FusionOS.Modules.Finance.Domain.TaxJurisdictions;
using FusionOS.Modules.Finance.Domain.TaxRates;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.Reports;

public class GetTaxPeriodSummaryReportQueryHandlerTests
{
    [Fact]
    public async Task Handle_GroupsActiveRatesUnderTheirJurisdiction_OrderedByCodeWithNullAmounts()
    {
        var companyId = Guid.NewGuid();
        var karnataka = TaxJurisdiction.Create(companyId, "IN-KA", "Karnataka");
        var california = TaxJurisdiction.Create(companyId, "US-CA", "California");

        var gst = TaxRate.Create(companyId, karnataka.Id, "GST-STANDARD", "GST 18%", 18m);
        var vat = TaxRate.Create(companyId, california.Id, "SALES-TAX", "CA Sales Tax", 7.25m);

        var jurisdictionRepository = Substitute.For<ITaxJurisdictionRepository>();
        jurisdictionRepository.ListAllActiveAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<TaxJurisdiction> { karnataka, california });

        var taxRateRepository = Substitute.For<ITaxRateRepository>();
        taxRateRepository.ListAllActiveAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<TaxRate> { gst, vat });

        var handler = new GetTaxPeriodSummaryReportQueryHandler(taxRateRepository, jurisdictionRepository);
        var periodStart = new DateTimeOffset(2026, 7, 1, 0, 0, 0, TimeSpan.Zero);
        var periodEnd = new DateTimeOffset(2026, 7, 31, 0, 0, 0, TimeSpan.Zero);

        var result = await handler.Handle(new GetTaxPeriodSummaryReportQuery(companyId, periodStart, periodEnd), CancellationToken.None);

        result.PeriodStart.Should().Be(periodStart);
        result.PeriodEnd.Should().Be(periodEnd);
        result.Lines.Should().HaveCount(2);

        // Ordered by jurisdiction code first — "IN-KA" sorts before "US-CA".
        result.Lines[0].TaxJurisdictionCode.Should().Be("IN-KA");
        result.Lines[0].TaxRateCode.Should().Be("GST-STANDARD");
        result.Lines[0].Percentage.Should().Be(18m);
        result.Lines[1].TaxJurisdictionCode.Should().Be("US-CA");
        result.Lines[1].TaxRateCode.Should().Be("SALES-TAX");

        // The real scope decision this report landed on: amounts are honestly
        // null (not derivable), never a fabricated zero.
        foreach (var line in result.Lines)
        {
            line.OutputTaxCollected.Should().BeNull();
            line.InputTaxPaid.Should().BeNull();
            line.NetTaxPayable.Should().BeNull();
        }

        result.TotalOutputTaxCollected.Should().BeNull();
        result.TotalInputTaxPaid.Should().BeNull();
        result.TotalNetTaxPayable.Should().BeNull();
        result.DataAvailabilityNote.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Handle_SkipsRatesWhoseJurisdictionIsNotAmongTheActiveSet()
    {
        var companyId = Guid.NewGuid();
        var activeJurisdiction = TaxJurisdiction.Create(companyId, "IN-KA", "Karnataka");
        var orphanRate = TaxRate.Create(companyId, Guid.NewGuid(), "ORPHAN", "Orphan Rate", 5m);
        var validRate = TaxRate.Create(companyId, activeJurisdiction.Id, "GST-STANDARD", "GST 18%", 18m);

        var jurisdictionRepository = Substitute.For<ITaxJurisdictionRepository>();
        jurisdictionRepository.ListAllActiveAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<TaxJurisdiction> { activeJurisdiction });

        var taxRateRepository = Substitute.For<ITaxRateRepository>();
        taxRateRepository.ListAllActiveAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<TaxRate> { orphanRate, validRate });

        var handler = new GetTaxPeriodSummaryReportQueryHandler(taxRateRepository, jurisdictionRepository);

        var result = await handler.Handle(
            new GetTaxPeriodSummaryReportQuery(companyId, DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow),
            CancellationToken.None);

        result.Lines.Should().ContainSingle();
        result.Lines[0].TaxRateCode.Should().Be("GST-STANDARD");
    }

    [Fact]
    public async Task Handle_WhenNoActiveTaxRates_ReturnsEmptyReportWithGapNoteStillPresent()
    {
        var companyId = Guid.NewGuid();

        var jurisdictionRepository = Substitute.For<ITaxJurisdictionRepository>();
        jurisdictionRepository.ListAllActiveAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<TaxJurisdiction>());

        var taxRateRepository = Substitute.For<ITaxRateRepository>();
        taxRateRepository.ListAllActiveAsync(companyId, Arg.Any<CancellationToken>())
            .Returns(new List<TaxRate>());

        var handler = new GetTaxPeriodSummaryReportQueryHandler(taxRateRepository, jurisdictionRepository);

        var result = await handler.Handle(
            new GetTaxPeriodSummaryReportQuery(companyId, DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow),
            CancellationToken.None);

        result.Lines.Should().BeEmpty();
        result.DataAvailabilityNote.Should().NotBeNullOrWhiteSpace();
    }
}
