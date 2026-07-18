using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using FusionOS.Modules.Finance.Application.CostCenters.Queries.ListCostCenters;
using FusionOS.Modules.Finance.Domain.CostCenters;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.CostCenters;

public class ListCostCentersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedCostCentersForTheCompany()
    {
        var companyId = Guid.NewGuid();
        var costCenters = new[] { CostCenter.Create(companyId, "CC-100", "Manufacturing") };
        var repository = Substitute.For<ICostCenterRepository>();
        repository.ListAsync(companyId, null, 1, 25, Arg.Any<CancellationToken>()).Returns(costCenters);
        repository.CountAsync(companyId, null, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListCostCentersQueryHandler(repository);

        var result = await handler.Handle(new ListCostCentersQuery(companyId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(c => c.Code == "CC-100");
    }
}
