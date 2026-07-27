using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Quotations.Commands.CreateQuotation;
using FusionOS.Modules.Sales.Application.Quotations.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Quotations.Commands.AcceptOwnQuotation;

public sealed class AcceptOwnQuotationCommandHandler : IRequestHandler<AcceptOwnQuotationCommand, QuotationDto>
{
    private readonly IQuotationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AcceptOwnQuotationCommandHandler(IQuotationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<QuotationDto> Handle(AcceptOwnQuotationCommand request, CancellationToken cancellationToken)
    {
        var quotation = await _repository.GetByIdForCustomerAsync(request.CustomerId, request.QuotationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Quotation '{request.QuotationId}' was not found.");

        quotation.Accept();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateQuotationCommandHandler.MapToDto(quotation);
    }
}
