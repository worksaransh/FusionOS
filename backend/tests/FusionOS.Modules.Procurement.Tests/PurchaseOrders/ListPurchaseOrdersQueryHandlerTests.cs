using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Queries.ListPurchaseOrders;
using FusionOS.Modules.Procurement.Domain.PurchaseOrders;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.PurchaseOrders;

public class ListPurchaseOrdersQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPagedOrdersForTheCompany()
    {
        var companyId = Guid.NewGuid();
        var order = PurchaseOrder.Create(companyId, Guid.NewGuid(), new[] { new PurchaseOrderLineInput(Guid.NewGuid(), 2m, 25m) });
        var repository = Substitute.For<IPurchaseOrderRepository>();
        repository.ListAsync(companyId, 1, 25, Arg.Any<CancellationToken>()).Returns(new[] { order });
        repository.CountAsync(companyId, Arg.Any<CancellationToken>()).Returns(1);
        var handler = new ListPurchaseOrdersQueryHandler(repository);

        var result = await handler.Handle(new ListPurchaseOrdersQuery(companyId), CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Data.Should().ContainSingle(o => o.TotalAmount == 50m);
    }
}
