using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using FusionOS.Modules.Inventory.Application.Transfers.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Transfers.Commands.CompleteTransfer;

/// <summary>
/// The only place a Transfer actually moves stock — Transfer.Complete() itself
/// only flips status and raises the domain event; this handler is responsible
/// for checking source stock and posting the two InventoryLedgerEntry rows
/// (source out, destination in) in the same unit of work, same "aggregate
/// raises the event, the Application-layer handler does the cross-aggregate
/// write" split as ReleaseReservation/FulfillReservation.
/// </summary>
public sealed class CompleteTransferCommandHandler : IRequestHandler<CompleteTransferCommand, TransferDto>
{
    private readonly ITransferRepository _repository;
    private readonly IInventoryLedgerRepository _ledgerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CompleteTransferCommandHandler(ITransferRepository repository, IInventoryLedgerRepository ledgerRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _ledgerRepository = ledgerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TransferDto> Handle(CompleteTransferCommand request, CancellationToken cancellationToken)
    {
        var transfer = await _repository.GetByIdAsync(request.CompanyId, request.TransferId, cancellationToken)
            ?? throw new KeyNotFoundException($"Transfer '{request.TransferId}' was not found.");

        var stockOnHand = await _ledgerRepository.SumQuantityAsync(request.CompanyId, transfer.ProductId, transfer.SourceWarehouseId, cancellationToken);
        if (stockOnHand < transfer.Quantity)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.TransferId),
                    $"Insufficient stock at the source warehouse: {stockOnHand} on hand, {transfer.Quantity} requested."),
            });
        }

        transfer.Complete();

        await _ledgerRepository.AddAsync(
            Domain.Ledger.InventoryLedgerEntry.RecordAdjustment(
                request.CompanyId, transfer.ProductId, transfer.SourceWarehouseId, -transfer.Quantity,
                $"Transfer {transfer.Id} to warehouse {transfer.DestinationWarehouseId}"),
            cancellationToken);
        await _ledgerRepository.AddAsync(
            Domain.Ledger.InventoryLedgerEntry.RecordAdjustment(
                request.CompanyId, transfer.ProductId, transfer.DestinationWarehouseId, transfer.Quantity,
                $"Transfer {transfer.Id} from warehouse {transfer.SourceWarehouseId}"),
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TransferMapper.ToDto(transfer);
    }
}
