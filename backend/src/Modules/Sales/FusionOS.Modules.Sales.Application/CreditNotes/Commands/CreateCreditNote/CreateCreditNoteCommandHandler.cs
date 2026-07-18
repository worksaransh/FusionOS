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

        // Mirrors CreateInvoiceCommandHandler's cumulative-quantity guard: reject any
        // line that would push the cumulative credited quantity for that product past
        // what the invoice actually billed for it.
        var failures = new List<FluentValidation.Results.ValidationFailure>();
        foreach (var line in request.Lines)
        {
            var invoiceLine = invoice.Lines.FirstOrDefault(l => l.ProductId == line.ProductId);
            if (invoiceLine is null)
            {
                failures.Add(new FluentValidation.Results.ValidationFailure(
                    nameof(request.Lines), $"Product {line.ProductId} is not part of invoice {request.InvoiceId}."));
                continue;
            }

            var alreadyCredited = await _repository.GetCreditedQuantityAsync(request.CompanyId, request.InvoiceId, line.ProductId, cancellationToken);
            if (alreadyCredited + line.Quantity > invoiceLine.Quantity)
            {
                failures.Add(new FluentValidation.Results.ValidationFailure(
                    nameof(request.Lines),
                    $"Product {line.ProductId}: crediting {line.Quantity} would exceed the invoice's remaining creditable quantity " +
                    $"({invoiceLine.Quantity - alreadyCredited} of {invoiceLine.Quantity} left, {alreadyCredited} already credited)."));
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
