using FusionOS.Modules.Sales.Application.SalesOrders.Commands.FlagSalesOrderLineBackordered;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using FusionOS.Modules.Sales.Domain.SalesOrders;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.SalesOrders;

public class FlagSalesOrderLineBackorderedCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidQuantity_FlagsLineAndReturnsDto()
    {
        var companyId = Guid.NewGuid();
        var order = SalesOrder.Create(companyId, Guid.NewGuid(), new[] { new SalesOrderLineInput(Guid.NewGuid(), 5m, 20m) });
        var lineId = order.Lines[0].Id;
        var repository = Substitute.For<ISalesOrderRepository>();
        repository.GetByIdAsync(companyId, order.Id, Arg.Any<CancellationToken>()).Returns(order);
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new FlagSalesOrderLineBackorderedCommandHandler(repository, unitOfWork);

        var result = await handler.Handle(new FlagSalesOrderLineBackorderedCommand(companyId, order.Id, lineId, 3m), CancellationToken.None);

        result.Lines.Single(l => l.Id == lineId).IsBackordered.Should().BeTrue();
        result.Lines.Single(l => l.Id == lineId).BackorderedQuantity.Should().Be(3m);
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
        var handler = new FlagSalesOrderLineBackorderedCommandHandler(repository, unitOfWork);

        var act = () => handler.Handle(new FlagSalesOrderLineBackorderedCommand(companyId, orderId, Guid.NewGuid(), 1m), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
