using FluentAssertions;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;
using FusionOS.Modules.Finance.Application.TaxRates.Queries.ListTaxRates;
using FusionOS.Modules.Finance.Domain.TaxRates;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.TaxRates;

public class ListTaxRatesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedTaxRatesForTheJurisdiction()
    {
        var companyId = Guid.NewGuid();
        var taxJurisdictionId = Guid.NewGuid();
        var taxRates = new[] { TaxRate.Create(companyId, taxJurisdictionId, "GST-STANDARD", "GST 18%", 18.00m) };
        var repository = Substitute.For<ITaxRateRepository>();
        repository.ListAsync(companyId, taxJurisdictionId, 1, 25, Arg.Any<CancellationToken>()).Returns(taxRates);
        repository.CountAsync(companyId, taxJurisdictionId, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListTaxRatesQueryHandler(repository);

        var result = await handler.Handle(new ListTaxRatesQuery(companyId, taxJurisdictionId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(r => r.Code == "GST-STANDARD");
    }
}
