using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.CreditNotes.Contracts;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.CreditNotes.Commands.CreateCreditNote;

public sealed class CreateCreditNoteCommandHandler : IRequestHandler<CreateCreditNoteCommand, CreditNoteDto>
{
    private readonly ICreditNoteRepository _repository;
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCreditNoteCommandHandler(ICreditNoteRepository repository, IInvoiceRepository invoiceRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _invoiceRepository = invoiceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreditNoteDto> Handle(CreateCreditNoteCommand request, CancellationToken cancellationToken)
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

        // Mirrors CreateInvoiceCommandHandler's cumulative-quantity guard (tightened
        // 2026-07-20, same pass as the Invoice/Dispatch handlers): the cumulative
        // credited quantity per product - every existing credit note against this
        // invoice plus every line of this request - must never exceed the quantity
        // the invoice actually billed. Request lines are grouped by product before
        // checking, so the same product split across several request lines cannot
        // slip past the cap by each line passing individually; the cap itself sums
        // every invoice line carrying the product, in case the invoice lists a
        // product more than once.
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
            var alreadyCredited = await _repository.GetCreditedQuantityAsync(request.CompanyId, request.InvoiceId, productLines.Key, cancellationToken);
            if (alreadyCredited + requestedQuantity > invoicedQuantity)
            {
                failures.Add(new FluentValidation.Results.ValidationFailure(
                    nameof(request.Lines),
                    $"Product {productLines.Key}: crediting {requestedQuantity} would exceed the invoice's remaining creditable quantity " +
                    $"({invoicedQuantity - alreadyCredited} of {invoicedQuantity} left, {alreadyCredited} already credited)."));
            }
        }

        if (failures.Count > 0)
            throw new ValidationException(failures);

        var creditNote = Domain.CreditNotes.CreditNote.Create(request.CompanyId, request.InvoiceId, request.CustomerId, request.Reason, request.Lines);

        await _repository.AddAsync(creditNote, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(creditNote);
    }

    internal static CreditNoteDto MapToDto(Domain.CreditNotes.CreditNote creditNote) => new(
        creditNote.Id,
        creditNote.InvoiceId,
        creditNote.CustomerId,
        creditNote.Reason,
        creditNote.Status.ToString(),
        creditNote.CreditNoteDate,
        creditNote.TotalAmount,
        creditNote.Lines.Select(l => new CreditNoteLineDto(l.Id, l.ProductId, l.Quantity, l.UnitPrice, l.LineTotal)).ToList());
}
