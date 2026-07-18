using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Application.Transfers.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Transfers.Commands.CancelTransfer;

public sealed class CancelTransferCommandHandler : IRequestHandler<CancelTransferCommand, TransferDto>
{
    private readonly ITransferRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelTransferCommandHandler(ITransferRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TransferDto> Handle(CancelTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _repository.GetByIdAsync(request.CompanyId, request.TransferId, cancellationToken)
            ?? throw new KeyNotFoundException($"Transfer '{request.TransferId}' was not found.");

        transfer.Cancel();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TransferMapper.ToDto(transfer);
    }
}
