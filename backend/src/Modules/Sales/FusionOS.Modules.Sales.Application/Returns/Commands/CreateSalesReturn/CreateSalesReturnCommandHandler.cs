using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;
using FusionOS.Modules.Sales.Application.Returns.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Returns.Commands.CreateSalesReturn;

public sealed class CreateSalesReturnCommandHandler : IRequestHandler<CreateSalesReturnCommand, SalesReturnDto>
{
    private readonly ISalesReturnRepository _repository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSalesReturnCommandHandler(ISalesReturnRepository repository, IInvoiceRepository invoiceRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SalesReturnDto> Handle(CreateSalesReturnCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(request.CompanyId, request.InvoiceId, cancellationToken);
        if (invoice is null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.InvoiceId), "Invoice does not exist for this company."),
            });
        }

        if (invoice.CustomerId != request.CustomerId)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.CustomerId), "Customer does not match the invoice's customer."),
            });
        }

        // Mirrors CreateCreditNoteCommandHandler's cumulative-quantity guard: the
        // cumulative returned quantity per product - every existing (non-Rejected)
        // sales return against this invoice plus every line of this request - must
        // never exceed the quantity the invoice actually billed. Request lines are
        // grouped by product before checking, so the same product split across
        // several request lines cannot slip past the cap by each line passing
        // individually; the cap itself sums every invoice line carrying the
        // product, in case the invoice lists a product more than once.
        var failures = new List<FluentValidation.Results.ValidationFailure>();
        foreach (var productLines in request.Lines.GroupBy(l => l.ProductId))
        {
            if (!invoice.Lines.Any(l => l.ProductId == productLines.Key))
            {
                failures.Add(new FluentValidation.Results.ValidationFailure(
                    nameof(request.Lines), $"Product {productLines.Key} is not part of invoice {request.InvoiceId}."));
                continue;
            }

            var invoicedQuantity = invoice.Lines.Where(l => l.ProductId == productLines.Key).Sum(l => l.Quantity);
            var requestedQuantity = productLines.Sum(l => l.Quantity);
            var alreadyReturned = await _repository.GetReturnedQuantityAsync(request.CompanyId, request.InvoiceId, productLines.Key, cancellationToken);
            if (alreadyReturned + requestedQuantity > invoicedQuantity)
            {
                failures.Add(new FluentValidation.Results.ValidationFailure(
                    nameof(request.Lines),
                    $"Product {productLines.Key}: returning {requestedQuantity} would exceed the invoice's remaining returnable quantity " +
                    $"({invoicedQuantity - alreadyReturned} of {invoicedQuantity} left, {alreadyReturned} already returned)."));
            }
        }

        if (failures.Count > 0)
            throw new ValidationException(failures);

        var salesReturn = Domain.Returns.SalesReturn.Create(request.CompanyId, request.InvoiceId, request.CustomerId, request.Reason, request.Lines);

        await _repository.AddAsync(salesReturn, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(salesReturn);
    }

    internal static SalesReturnDto MapToDto(Domain.Returns.SalesReturn salesReturn) => new(
        salesReturn.Id,
        salesReturn.InvoiceId,
        salesReturn.CustomerId,
        salesReturn.Reason,
        salesReturn.Status.ToString(),
        salesReturn.RequestDate,
        salesReturn.RejectionReason,
        salesReturn.Lines.Select(l => new SalesReturnLineDto(l.Id, l.ProductId, l.Quantity, l.ConditionNotes, l.Restocked)).ToList());
}
