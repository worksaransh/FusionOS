using FluentAssertions;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;
using FusionOS.Modules.Finance.Application.TaxRates.Queries.CalculateLineTax;
using FusionOS.Modules.Finance.Domain.TaxRates;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.TaxRates;

public class CalculateLineTaxQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenTaxRateExists_ComputesTaxAndGross()
    {
        var companyId = Guid.NewGuid();
        var taxRate = TaxRate.Create(companyId, Guid.NewGuid(), "GST-STANDARD", "GST 18%", 18.00m);
        var repository = Substitute.For<ITaxRateRepository>();
        repository.GetByIdAsync(companyId, taxRate.Id, Arg.Any<CancellationToken>()).Returns(taxRate);
        var handler = new CalculateLineTaxQueryHandler(repository);

        var result = await handler.Handle(new CalculateLineTaxQuery(companyId, taxRate.Id, 100m), CancellationToken.None);

        result.Percentage.Should().Be(18.00m);
        result.TaxAmount.Should().Be(18.0000m);
        result.GrossAmount.Should().Be(118.0000m);
        result.TaxRateCode.Should().Be("GST-STANDARD");
    }

    [Fact]
    public async Task Handle_RoundsTaxToFourDecimalPlaces()
    {
        var companyId = Guid.NewGuid();
        var taxRate = TaxRate.Create(companyId, Guid.NewGuid(), "GST-REDUCED", "GST 5%", 5.00m);
        var repository = Substitute.For<ITaxRateRepository>();
        repository.GetByIdAsync(companyId, taxRate.Id, Arg.Any<CancellationToken>()).Returns(taxRate);
        var handler = new CalculateLineTaxQueryHandler(repository);

        var result = await handler.Handle(new CalculateLineTaxQuery(companyId, taxRate.Id, 33.333m), CancellationToken.None);

        // 33.333 * 5% = 1.66665 -> rounded away from zero at scale 4 = 1.6667
        result.TaxAmount.Should().Be(1.6667m);
    }

    [Fact]
    public async Task Handle_WhenTaxRateDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var taxRateId = Guid.NewGuid();
        var repository = Substitute.For<ITaxRateRepository>();
        repository.GetByIdAsync(companyId, taxRateId, Arg.Any<CancellationToken>()).Returns((TaxRate?)null);
        var handler = new CalculateLineTaxQueryHandler(repository);

        var act = () => handler.Handle(new CalculateLineTaxQuery(companyId, taxRateId, 100m), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
