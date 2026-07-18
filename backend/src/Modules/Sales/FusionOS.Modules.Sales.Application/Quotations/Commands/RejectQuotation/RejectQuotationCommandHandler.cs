using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Quotations.Commands.CreateQuotation;
using FusionOS.Modules.Sales.Application.Quotations.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Quotations.Commands.RejectQuotation;

public sealed class RejectQuotationCommandHandler : IRequestHandler<RejectQuotationCommand, QuotationDto>
{
    private readonly IQuotationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RejectQuotationCommandHandler(IQuotationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<QuotationDto> Handle(RejectQuotationCommand request, CancellationToken cancellationToken)
    {
        var quotation = await _repository.GetByIdAsync(request.CompanyId, request.QuotationId, cancellationToken)
            ?? throw new KeyNotFoundException($"Quotation '{request.QuotationId}' was not found.");

        quotation.Reject();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateQuotationCommandHandler.MapToDto(quotation);
    }
}
