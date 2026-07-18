using FluentAssertions;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Contracts;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Queries.ListTaxJurisdictions;
using FusionOS.Modules.Finance.Domain.TaxJurisdictions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.TaxJurisdictions;

public class ListTaxJurisdictionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedTaxJurisdictionsForTheCompany()
    {
        var companyId = Guid.NewGuid();
        var jurisdictions = new[] { TaxJurisdiction.Create(companyId, "IN-KA", "Karnataka, India") };
        var repository = Substitute.For<ITaxJurisdictionRepository>();
        repository.ListAsync(companyId, null, 1, 25, Arg.Any<CancellationToken>()).Returns(jurisdictions);
        repository.CountAsync(companyId, null, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListTaxJurisdictionsQueryHandler(repository);

        var result = await handler.Handle(new ListTaxJurisdictionsQuery(companyId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(j => j.Code == "IN-KA");
    }
}
