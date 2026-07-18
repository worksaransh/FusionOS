using FusionOS.Modules.Procurement.Application.PurchaseOrders.Commands.CreatePurchaseOrder;
using FusionOS.Modules.Procurement.Application.PurchaseOrders.Contracts;
using FusionOS.Modules.Procurement.Application.Rfqs.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using FusionOS.Modules.Procurement.Domain.PurchaseOrders;
using FusionOS.Modules.Procurement.Domain.Rfqs;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.Rfqs.Commands.ConvertRfqToPurchaseOrder;

public sealed class ConvertRfqToPurchaseOrderCommandHandler : IRequestHandler<ConvertRfqToPurchaseOrderCommand, PurchaseOrderDto>
{
    private readonly IRfqRepository _rfqRepository;
    private readonly IPurchaseOrderRepository _purchaseOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConvertRfqToPurchaseOrderCommandHandler(IRfqRepository rfqRepository, IPurchaseOrderRepository purchaseOrderRepository, IUnitOfWork unitOfWork)
    {
        _rfqRepository = rfqRepository;
        _purchaseOrderRepository = purchaseOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PurchaseOrderDto> Handle(ConvertRfqToPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var rfq = await _rfqRepository.GetByIdAsync(request.CompanyId, request.RfqId, cancellationToken)
            ?? throw new KeyNotFoundException($"RFQ '{request.RfqId}' was not found.");

        // Guard checked here (not just inside MarkConverted) because building the
        // PurchaseOrderLineInput list below needs the awarded quote's lines before
        // MarkConverted is ever called — same guard-before-create ordering as
        // ConvertQuotationToSalesOrderCommandHandler, so an invalid conversion
        // never leaves an orphaned PurchaseOrder behind.
        if (rfq.Status != RfqStatus.Awarded)
            throw new InvalidOperationException($"Only an Awarded RFQ can be converted (current status: {rfq.Status}).");
        if (rfq.ConvertedPurchaseOrderId is not null)
            throw new InvalidOperationException("This RFQ has already been converted into a purchase order.");

        var awardedQuote = rfq.SupplierQuotes.First(q => q.Id == rfq.AwardedSupplierQuoteId);

        var lines = awardedQuote.Lines
            .Select(l => new PurchaseOrderLineInput(l.ProductId, l.Quantity, l.UnitPrice))
            .ToList();
        var purchaseOrder = Domain.PurchaseOrders.PurchaseOrder.Create(request.CompanyId, awardedQuote.SupplierId, lines);

        rfq.MarkConverted(purchaseOrder.Id);

        await _purchaseOrderRepository.AddAsync(purchaseOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken); // commits both in one save

        return CreatePurchaseOrderCommandHandler.MapToDto(purchaseOrder);
    }
}
