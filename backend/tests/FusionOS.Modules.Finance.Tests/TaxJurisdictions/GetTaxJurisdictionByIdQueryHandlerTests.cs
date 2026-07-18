using FluentAssertions;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Contracts;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Queries.GetTaxJurisdictionById;
using FusionOS.Modules.Finance.Domain.TaxJurisdictions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.TaxJurisdictions;

public class GetTaxJurisdictionByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenTaxJurisdictionExists_ReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var jurisdiction = TaxJurisdiction.Create(companyId, "IN-KA", "Karnataka, India");
        var repository = Substitute.For<ITaxJurisdictionRepository>();
        repository.GetByIdAsync(companyId, jurisdiction.Id, Arg.Any<CancellationToken>()).Returns(jurisdiction);
        var handler = new GetTaxJurisdictionByIdQueryHandler(repository);

        var result = await handler.Handle(new GetTaxJurisdictionByIdQuery(companyId, jurisdiction.Id), CancellationToken.None);

        result.Code.Should().Be("IN-KA");
    }

    [Fact]
    public async Task Handle_WhenTaxJurisdictionDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var jurisdictionId = Guid.NewGuid();
        var repository = Substitute.For<ITaxJurisdictionRepository>();
        repository.GetByIdAsync(companyId, jurisdictionId, Arg.Any<CancellationToken>()).Returns((TaxJurisdiction?)null);
        var handler = new GetTaxJurisdictionByIdQueryHandler(repository);

        var act = () => handler.Handle(new GetTaxJurisdictionByIdQuery(companyId, jurisdictionId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
