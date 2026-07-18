using FusionOS.Modules.Procurement.Application.Rfqs.Commands.CreateRfq;
using FusionOS.Modules.Procurement.Application.Rfqs.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.Rfqs.Commands.SendRfq;

public sealed class SendRfqCommandHandler : IRequestHandler<SendRfqCommand, RfqDto>
{
    private readonly IRfqRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SendRfqCommandHandler(IRfqRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<RfqDto> Handle(SendRfqCommand request, CancellationToken cancellationToken)
    {
        var rfq = await _repository.GetByIdAsync(request.CompanyId, request.RfqId, cancellationToken)
            ?? throw new KeyNotFoundException($"RFQ '{request.RfqId}' was not found.");

        rfq.Send();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateRfqCommandHandler.MapToDto(rfq);
    }
}
