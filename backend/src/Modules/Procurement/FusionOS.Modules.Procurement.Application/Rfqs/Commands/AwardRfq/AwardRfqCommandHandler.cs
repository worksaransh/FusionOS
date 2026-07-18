using FusionOS.Modules.Procurement.Application.Rfqs.Commands.CreateRfq;
using FusionOS.Modules.Procurement.Application.Rfqs.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.Rfqs.Commands.AwardRfq;

public sealed class AwardRfqCommandHandler : IRequestHandler<AwardRfqCommand, RfqDto>
{
    private readonly IRfqRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AwardRfqCommandHandler(IRfqRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RfqDto> Handle(AwardRfqCommand request, CancellationToken cancellationToken)
    {
        var rfq = await _repository.GetByIdAsync(request.CompanyId, request.RfqId, cancellationToken)
            ?? throw new KeyNotFoundException($"RFQ '{request.RfqId}' was not found.");

        rfq.Award(request.SupplierQuoteId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateRfqCommandHandler.MapToDto(rfq);
    }
}
