using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;
using FusionOS.Modules.Sales.Application.SalesOrders.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Invoices.Commands.CreateInvoice;

public sealed class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, InvoiceDto>
{
    private readonly IInvoiceRepository _repository;
    private readonly ISalesOrderRepository _salesOrderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInvoiceCommandHandler(IInvoiceRepository repository, ISalesOrderRepository salesOrderRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _salesOrderRepository = salesOrderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<InvoiceDto> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var salesOrder = await _salesOrderRepository.GetByIdAsync(request.CompanyId, request.SalesOrderId, cancellationToken);
        if (salesOrder is null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.SalesOrderId), "Sales order does not exist for this company."),
            });
        }

        // 2026-07-14 coverage-audit follow-up: previously nothing compared the
        // requested invoice lines to what the sales order actually ordered, or to
        // what had already been invoiced against it - an order could be invoiced
        // for any quantity, any number of times. Reject any line that would push
        // the cumulative invoiced quantity for that product past what was ordered.
        var failures = new List<FluentValidation.Results.ValidationFailure>();
        foreach (var line in request.Lines)
        {
            var orderLine = salesOrder.Lines.FirstOrDefault(l => l.ProductId == line.ProductId);
            if (orderLine is null)
            {
                failures.Add(new FluentValidation.Results.ValidationFailure(
                    nameof(request.Lines), $"Product {line.ProductId} is not part of sales order {request.SalesOrderId}."));
                continue;
            }

            var alreadyInvoiced = await _repository.GetInvoicedQuantityAsync(request.CompanyId, request.SalesOrderId, line.ProductId, cancellationToken);
            if (alreadyInvoiced + line.Quantity > orderLine.Quantity)
            {
                failures.Add(new FluentValidation.Results.ValidationFailure(
                    nameof(request.Lines),
                    $"Product {line.ProductId}: invoicing {line.Quantity} would exceed the sales order's remaining invoiceable quantity " +
                    $"({orderLine.Quantity - alreadyInvoiced} of {orderLine.Quantity} left, {alreadyInvoiced} already invoiced)."));
            }
        }

        if (failures.Count > 0)
            throw new ValidationException(failures);

        var invoice = Domain.Invoices.Invoice.Create(request.CompanyId, request.SalesOrderId, request.CustomerId, request.Lines, request.SalesPersonId);

        await _repository.AddAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(invoice);
    }

    internal static InvoiceDto MapToDto(Domain.Invoices.Invoice invoice) => new(
        invoice.Id,
        invoice.SalesOrderId,
        invoice.CustomerId,
        invoice.Status.ToString(),
        invoice.InvoiceDate,
        invoice.TotalAmount,
        invoice.Lines.Select(l => new InvoiceLineDto(l.Id, l.ProductId, l.Quantity, l.UnitPrice, l.LineTotal, l.TaxRateId, l.TaxAmount)).ToList(),
        invoice.SalesPersonId);
}
