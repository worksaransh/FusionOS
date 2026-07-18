using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Quotations.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Quotations.Commands.CreateQuotation;

public sealed class CreateQuotationCommandHandler : IRequestHandler<CreateQuotationCommand, QuotationDto>
{
    private readonly IQuotationRepository _repository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateQuotationCommandHandler(IQuotationRepository repository, ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<QuotationDto> Handle(CreateQuotationCommand request, CancellationToken cancellationToken)
    {
        if (!await _customerRepository.ExistsAsync(request.CompanyId, request.CustomerId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.CustomerId), "Customer does not exist for this company."),
            });
        }

        var quotation = Domain.Quotations.Quotation.Create(request.CompanyId, request.CustomerId, request.Lines);

        await _repository.AddAsync(quotation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(quotation);
    }

    internal static QuotationDto MapToDto(Domain.Quotations.Quotation quotation) => new(
        quotation.Id,
        quotation.CustomerId,
        quotation.Status.ToString(),
        quotation.QuotationDate,
        quotation.ConvertedSalesOrderId,
        quotation.TotalAmount,
        quotation.Lines.Select(l => new QuotationLineDto(l.Id, l.ProductId, l.Quantity, l.UnitPrice, l.LineTotal)).ToList());
}
