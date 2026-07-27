using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using FusionOS.Modules.Sales.Application.SalesOrders.Queries.ListMySalesOrders;
using FusionOS.Modules.Sales.Domain.SalesOrders;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.SalesOrders;

/// <summary>Cross-customer leakage check for the portal - see ListMyQuotationsQueryHandlerTests for the full rationale.</summary>
public class ListMySalesOrdersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsOnlyTheRequestingCustomersOwnSalesOrders()
    {
        var companyId = Guid.NewGuid();
        var customerA = Guid.NewGuid();
        var customerB = Guid.NewGuid();

        var orderForA = SalesOrder.Create(companyId, customerA, new[] { new SalesOrderLineInput(Guid.NewGuid(), 1m, 100m) });
        var orderForB = SalesOrder.Create(companyId, customerB, new[] { new SalesOrderLineInput(Guid.NewGuid(), 1m, 200m) });

        var repository = Substitute.For<ISalesOrderRepository>();
        repository.ListByCustomerAsync(customerA, 1, 25, Arg.Any<CancellationToken>()).Returns(new[] { orderForA });
        repository.CountByCustomerAsync(customerA, Arg.Any<CancellationToken>()).Returns(1);
        repository.ListByCustomerAsync(customerB, 1, 25, Arg.Any<CancellationToken>()).Returns(new[] { orderForB });
        repository.CountByCustomerAsync(customerB, Arg.Any<CancellationToken>()).Returns(1);

        var handler = new ListMySalesOrdersQueryHandler(repository);

        var resultForA = await handler.Handle(new ListMySalesOrdersQuery(customerA), CancellationToken.None);
        var resultForB = await handler.Handle(new ListMySalesOrdersQuery(customerB), CancellationToken.None);

        resultForA.Data.Should().ContainSingle(o => o.Id == orderForA.Id);
        resultForA.Data.Should().NotContain(o => o.Id == orderForB.Id);
        resultForB.Data.Should().ContainSingle(o => o.Id == orderForB.Id);
        resultForB.Data.Should().NotContain(o => o.Id == orderForA.Id);

        await repository.DidNotReceive().ListAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}
