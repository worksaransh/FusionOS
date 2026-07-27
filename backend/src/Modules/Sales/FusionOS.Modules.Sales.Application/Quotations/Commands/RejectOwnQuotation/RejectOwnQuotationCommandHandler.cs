using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Quotations.Commands.CreateQuotation;
using FusionOS.Modules.Sales.Application.Quotations.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Quotations.Commands.RejectOwnQuotation;

public sealed class RejectOwnQuotationCommandHandler : IRequestHandler<RejectOwnQuotationCommand, QuotationDto>
{
    private readonly IQuotationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RejectOwnQuotationCommandHandler(IQuotationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<QuotationDto> Handle(RejectOwnQuotationCommand request, CancellationToken cancellationToken)
    {
        var quotation = await _repository.GetByIdForCustomerAsync(request.CustomerId, request.QuotationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Quotation '{request.QuotationId}' was not found.");

        quotation.Reject();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateQuotationCommandHandler.MapToDto(quotation);
    }
}
