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

        // Cross-aggregate quantity guard (2026-07-14 coverage-audit follow-up,
        // tightened 2026-07-20): the cumulative invoiced quantity per product -
        // every existing invoice against this sales order plus every line of this
        // request - must never exceed the quantity the order actually ordered.
        //
        // Counting rule: ALL persisted invoices count toward the cap, Draft and
        // Issued alike. InvoiceStatus has no cancelled/voided state today, so
        // every persisted invoice is a live claim on the ordered quantity (a
        // Draft is on its way to being Issued, not abandoned). If a cancellation
        // status is ever introduced, IInvoiceRepository.GetInvoicedQuantityAsync
        // must be updated to exclude it - the decision lives in that query, not here.
        //
        // Request lines are grouped by product before checking, so the same
        // product split across several request lines cannot slip past the cap by
        // each line passing individually; the cap itself sums every order line
        // carrying the product, in case the order lists a product more than once.
        var failures = new List<FluentValidation.Results.ValidationFailure>();
        foreach (var productLines in request.Lines.GroupBy(l => l.ProductId))
        {
            if (!salesOrder.Lines.Any(l => l.ProductId == productLines.Key))
            {
                failures.Add(new FluentValidation.Results.ValidationFailure(
                    nameof(request.Lines), $"Product {productLines.Key} is not part of sales order {request.SalesOrderId}."));
                continue;
            }

            var orderedQuantity = salesOrder.Lines.Where(l => l.ProductId == productLines.Key).Sum(l => l.Quantity);
            var requestedQuantity = productLines.Sum(l => l.Quantity);
            var alreadyInvoiced = await _repository.GetInvoicedQuantityAsync(request.CompanyId, request.SalesOrderId, productLines.Key, cancellationToken);
            if (alreadyInvoiced + requestedQuantity > orderedQuantity)
            {
                failures.Add(new FluentValidation.Results.ValidationFailure(
                    nameof(request.Lines),
                    $"Product {productLines.Key}: invoicing {requestedQuantity} would exceed the sales order's remaining invoiceable quantity " +
                    $"({orderedQuantity - alreadyInvoiced} of {orderedQuantity} left, {alreadyInvoiced} already invoiced)."));
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
