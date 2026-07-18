using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using FusionOS.Modules.Sales.Application.SalesOrders.Queries.ListSalesOrders;
using FusionOS.Modules.Sales.Domain.SalesOrders;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.SalesOrders;

public class ListSalesOrdersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedOrdersForTheCompany()
    {
        var companyId = Guid.NewGuid();
        var order = SalesOrder.Create(companyId, Guid.NewGuid(), new[] { new SalesOrderLineInput(Guid.NewGuid(), 3m, 20m) });
        var repository = Substitute.For<ISalesOrderRepository>();
        repository.ListAsync(companyId, 1, 25, Arg.Any<CancellationToken>()).Returns(new[] { order });
        repository.CountAsync(companyId, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListSalesOrdersQueryHandler(repository);

        var result = await handler.Handle(new ListSalesOrdersQuery(companyId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(o => o.TotalAmount == 60m);
    }
}
