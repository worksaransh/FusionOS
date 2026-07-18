using FusionOS.Modules.Sales.Application.Quotations.Commands.ConvertQuotationToSalesOrder;
using FusionOS.Modules.Sales.Application.Quotations.Contracts;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using FusionOS.Modules.Sales.Domain.Quotations;
using FusionOS.Modules.Sales.Domain.SalesOrders;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Sales.Tests.Quotations;

public class ConvertQuotationToSalesOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenQuotationIsAccepted_CreatesSalesOrderAndMarksConverted()
    {
        var companyId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var quotation = Quotation.Create(companyId, customerId, new[] { new QuotationLineInput(productId, 2m, 50m) });
        quotation.Accept();
        var quotationRepository = Substitute.For<IQuotationRepository>();
        quotationRepository.GetByIdAsync(companyId, quotation.Id, Arg.Any<CancellationToken>()).Returns(quotation);
        var salesOrderRepository = Substitute.For<ISalesOrderRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ConvertQuotationToSalesOrderCommandHandler(quotationRepository, salesOrderRepository, unitOfWork);

        var result = await handler.Handle(new ConvertQuotationToSalesOrderCommand(companyId, quotation.Id), CancellationToken.None);

        result.CustomerId.Should().Be(customerId);
        result.TotalAmount.Should().Be(100m);
        quotation.Status.Should().Be(QuotationStatus.Converted);
        quotation.ConvertedSalesOrderId.Should().Be(result.Id);
        await salesOrderRepository.Received(1).AddAsync(Arg.Any<SalesOrder>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenQuotationIsStillDraft_ThrowsInvalidOperationExceptionAndDoesNotSave()
    {
        var companyId = Guid.NewGuid();
        var quotation = Quotation.Create(companyId, Guid.NewGuid(), new[] { new QuotationLineInput(Guid.NewGuid(), 2m, 50m) });
        var quotationRepository = Substitute.For<IQuotationRepository>();
        quotationRepository.GetByIdAsync(companyId, quotation.Id, Arg.Any<CancellationToken>()).Returns(quotation);
        var salesOrderRepository = Substitute.For<ISalesOrderRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ConvertQuotationToSalesOrderCommandHandler(quotationRepository, salesOrderRepository, unitOfWork);

        var act = () => handler.Handle(new ConvertQuotationToSalesOrderCommand(companyId, quotation.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await salesOrderRepository.DidNotReceive().AddAsync(Arg.Any<SalesOrder>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenQuotationDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var quotationId = Guid.NewGuid();
        var quotationRepository = Substitute.For<IQuotationRepository>();
        quotationRepository.GetByIdAsync(companyId, quotationId, Arg.Any<CancellationToken>()).Returns((Quotation?)null);
        var salesOrderRepository = Substitute.For<ISalesOrderRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ConvertQuotationToSalesOrderCommandHandler(quotationRepository, salesOrderRepository, unitOfWork);

        var act = () => handler.Handle(new ConvertQuotationToSalesOrderCommand(companyId, quotationId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
