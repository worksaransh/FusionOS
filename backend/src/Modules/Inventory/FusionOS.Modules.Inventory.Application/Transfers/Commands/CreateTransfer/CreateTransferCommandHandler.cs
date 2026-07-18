using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Application.Transfers.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Transfers.Commands.CreateTransfer;

public sealed class CreateTransferCommandHandler : IRequestHandler<CreateTransferCommand, TransferDto>
{
    private readonly ITransferRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTransferCommandHandler(ITransferRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TransferDto> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = Domain.Transfers.Transfer.Create(
            request.CompanyId, request.ProductId, request.SourceWarehouseId, request.DestinationWarehouseId, request.Quantity);

        await _repository.AddAsync(transfer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TransferMapper.ToDto(transfer);
    }
}
