using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using FusionOS.Modules.Finance.Application.CostCenters.Queries.GetCostCenterById;
using FusionOS.Modules.Finance.Domain.CostCenters;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Finance.Tests.CostCenters;

public class GetCostCenterByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenCostCenterExists_ReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var costCenter = CostCenter.Create(companyId, "CC-100", "Manufacturing");
        var repository = Substitute.For<ICostCenterRepository>();
        repository.GetByIdAsync(companyId, costCenter.Id, Arg.Any<CancellationToken>()).Returns(costCenter);
        var handler = new GetCostCenterByIdQueryHandler(repository);

        var result = await handler.Handle(new GetCostCenterByIdQuery(companyId, costCenter.Id), CancellationToken.None);

        result.Code.Should().Be("CC-100");
    }

    [Fact]
    public async Task Handle_WhenCostCenterDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var costCenterId = Guid.NewGuid();
        var repository = Substitute.For<ICostCenterRepository>();
        repository.GetByIdAsync(companyId, costCenterId, Arg.Any<CancellationToken>()).Returns((CostCenter?)null);
        var handler = new GetCostCenterByIdQueryHandler(repository);

        var act = () => handler.Handle(new GetCostCenterByIdQuery(companyId, costCenterId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
