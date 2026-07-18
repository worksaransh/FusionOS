using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Quotations.Contracts;
using FusionOS.Modules.Sales.Application.SalesOrders.Commands.CreateSalesOrder;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using FusionOS.Modules.Sales.Domain.SalesOrders;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Quotations.Commands.ConvertQuotationToSalesOrder;

public sealed class ConvertQuotationToSalesOrderCommandHandler : IRequestHandler<ConvertQuotationToSalesOrderCommand, SalesOrderDto>
{
    private readonly IQuotationRepository _quotationRepository;
    private readonly ISalesOrderRepository _salesOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConvertQuotationToSalesOrderCommandHandler(
        IQuotationRepository quotationRepository,
        ISalesOrderRepository salesOrderRepository,
        IUnitOfWork unitOfWork)
    {
        _quotationRepository = quotationRepository;
        _salesOrderRepository = salesOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SalesOrderDto> Handle(ConvertQuotationToSalesOrderCommand request, CancellationToken cancellationToken)
    {
        var quotation = await _quotationRepository.GetByIdAsync(request.CompanyId, request.QuotationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Quotation '{request.QuotationId}' was not found.");

        var lines = quotation.Lines
            .Select(l => new SalesOrderLineInput(l.ProductId, l.Quantity, l.UnitPrice))
            .ToList();
        var salesOrder = Domain.SalesOrders.SalesOrder.Create(request.CompanyId, quotation.CustomerId, lines);

        // quotation.MarkConverted throws InvalidOperationException if the quotation
        // isn't Accepted - checked before the new SalesOrder is persisted below, so
        // an invalid conversion attempt never leaves an orphaned SalesOrder behind.
        quotation.MarkConverted(salesOrder.Id);

        await _salesOrderRepository.AddAsync(salesOrder, cancellationToken);
        // Same module, same DbContext as the Quotation repository - one
        // SaveChangesAsync commits both the new SalesOrder and the Quotation's
        // updated Status/ConvertedSalesOrderId, same restraint as the Approval
        // engine creating a Notification row directly in the same transaction.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateSalesOrderCommandHandler.MapToDto(salesOrder);
    }
}
