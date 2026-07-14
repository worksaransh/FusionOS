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

        var invoice = Domain.Invoices.Invoice.Create(request.CompanyId, request.SalesOrderId, request.CustomerId, request.Lines);

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
        invoice.Lines.Select(l => new InvoiceLineDto(l.Id, l.ProductId, l.Quantity, l.UnitPrice, l.LineTotal)).ToList());
}
