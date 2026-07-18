using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Rfqs.Commands.ConvertRfqToPurchaseOrder;
using FusionOS.Modules.Procurement.Application.Rfqs.Contracts;
using FusionOS.Modules.Procurement.Domain.PurchaseOrders;
using FusionOS.Modules.Procurement.Domain.Rfqs;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace FusionOS.Modules.Procurement.Tests.Rfqs;

public class ConvertRfqToPurchaseOrderCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenRfqIsAwarded_CreatesPurchaseOrderAndMarksConverted()
    {
        var companyId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var rfq = RequestForQuotation.Create(companyId, new[] { new RfqLineInput(productId, 2m) });
        rfq.Send();
        var quote = rfq.SubmitSupplierQuote(supplierId, new[] { new SupplierQuoteLineInput(productId, 2m, 50m) });
        rfq.Award(quote.Id);
        var rfqRepository = Substitute.For<IRfqRepository>();
        rfqRepository.GetByIdAsync(companyId, rfq.Id, Arg.Any<CancellationToken>()).Returns(rfq);
        var purchaseOrderRepository = Substitute.For<IPurchaseOrderRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ConvertRfqToPurchaseOrderCommandHandler(rfqRepository, purchaseOrderRepository, unitOfWork);

        var result = await handler.Handle(new ConvertRfqToPurchaseOrderCommand(companyId, rfq.Id), CancellationToken.None);

        result.SupplierId.Should().Be(supplierId);
        result.TotalAmount.Should().Be(100m);
        rfq.ConvertedPurchaseOrderId.Should().Be(result.Id);
        await purchaseOrderRepository.Received(1).AddAsync(Arg.Any<PurchaseOrder>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRfqIsStillSent_ThrowsInvalidOperationExceptionAndDoesNotSave()
    {
        var companyId = Guid.NewGuid();
        var rfq = RequestForQuotation.Create(companyId, new[] { new RfqLineInput(Guid.NewGuid(), 2m) });
        rfq.Send();
        var rfqRepository = Substitute.For<IRfqRepository>();
        rfqRepository.GetByIdAsync(companyId, rfq.Id, Arg.Any<CancellationToken>()).Returns(rfq);
        var purchaseOrderRepository = Substitute.For<IPurchaseOrderRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ConvertRfqToPurchaseOrderCommandHandler(rfqRepository, purchaseOrderRepository, unitOfWork);

        var act = () => handler.Handle(new ConvertRfqToPurchaseOrderCommand(companyId, rfq.Id), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
        await purchaseOrderRepository.DidNotReceive().AddAsync(Arg.Any<PurchaseOrder>(), Arg.Any<CancellationToken>());
        await unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRfqDoesNotExist_ThrowsKeyNotFoundException()
    {
        var companyId = Guid.NewGuid();
        var rfqId = Guid.NewGuid();
        var rfqRepository = Substitute.For<IRfqRepository>();
        rfqRepository.GetByIdAsync(companyId, rfqId, Arg.Any<CancellationToken>()).Returns((RequestForQuotation?)null);
        var purchaseOrderRepository = Substitute.For<IPurchaseOrderRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new ConvertRfqToPurchaseOrderCommandHandler(rfqRepository, purchaseOrderRepository, unitOfWork);

        var act = () => handler.Handle(new ConvertRfqToPurchaseOrderCommand(companyId, rfqId), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
