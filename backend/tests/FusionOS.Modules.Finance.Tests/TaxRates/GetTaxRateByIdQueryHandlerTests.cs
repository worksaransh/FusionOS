using FluentAssertions;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;
using FusionOS.Modules.Finance.Application.TaxRates.Queries.GetTaxRateById;
using FusionOS.Modules.Finance.Domain.TaxRates;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.TaxRates;

public class GetTaxRateByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenTaxRateExists_ReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var taxRate = TaxRate.Create(companyId, Guid.NewGuid(), "GST-STANDARD", "GST 18%", 18.00m);
        var repository = Substitute.For<ITaxRateRepository>();
        repository.GetByIdAsync(companyId, taxRate.Id, Arg.Any<CancellationToken>()).Returns(taxRate);
        var handler = new GetTaxRateByIdQueryHandler(repository);

        var result = await handler.Handle(new GetTaxRateByIdQuery(companyId, taxRate.Id), CancellationToken.None);

        result.Code.Should().Be("GST-STANDARD");
    }

    [Fact]
    public async Task Handle_WhenTaxRateDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var taxRateId = Guid.NewGuid();
        var repository = Substitute.For<ITaxRateRepository>();
        repository.GetByIdAsync(companyId, taxRateId, Arg.Any<CancellationToken>()).Returns((TaxRate?)null);
        var handler = new GetTaxRateByIdQueryHandler(repository);

        var act = () => handler.Handle(new GetTaxRateByIdQuery(companyId, taxRateId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
