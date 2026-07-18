using FusionOS.Modules.Sales.Application.SalesOrders.Commands.ClearSalesOrderLineBackorder;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using FusionOS.Modules.Sales.Domain.SalesOrders;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.SalesOrders;

public class ClearSalesOrderLineBackorderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenLineIsBackordered_ClearsFlagAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var order = SalesOrder.Create(companyId, Guid.NewGuid(), new[] { new SalesOrderLineInput(Guid.NewGuid(), 5m, 20m) });
        var lineId = order.Lines[0].Id;
        order.FlagLineBackordered(lineId, 3m);
        var repository = Substitute.For<ISalesOrderRepository>();
        repository.GetByIdAsync(companyId, order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ClearSalesOrderLineBackorderCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new ClearSalesOrderLineBackorderCommand(companyId, order.Id, lineId), CancellationToken.None);

        result.Lines.Single(l => l.Id == lineId).IsBackordered.Should().BeFalse();
        result.Lines.Single(l => l.Id == lineId).BackorderedQuantity.Should().Be(0m);
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenOrderDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var repository = Substitute.For<ISalesOrderRepository>();
        repository.GetByIdAsync(companyId, orderId, Arg.Any<CancellationToken>()).Returns((SalesOrder?)null);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ClearSalesOrderLineBackorderCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new ClearSalesOrderLineBackorderCommand(companyId, orderId, Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
