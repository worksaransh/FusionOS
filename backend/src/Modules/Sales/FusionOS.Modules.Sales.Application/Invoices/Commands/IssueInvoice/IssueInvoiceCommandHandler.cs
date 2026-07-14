using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Invoices.Commands.CreateInvoice;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Invoices.Commands.IssueInvoice;

public sealed class IssueInvoiceCommandHandler : IRequestHandler<IssueInvoiceCommand, InvoiceDto>
{
    private readonly IInvoiceRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public IssueInvoiceCommandHandler(IInvoiceRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<InvoiceDto> Handle(IssueInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await _repository.GetByIdAsync(request.CompanyId, request.InvoiceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Invoice '{request.InvoiceId}' was not found.");

        invoice.Issue();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateInvoiceCommandHandler.MapToDto(invoice);
    }
}
