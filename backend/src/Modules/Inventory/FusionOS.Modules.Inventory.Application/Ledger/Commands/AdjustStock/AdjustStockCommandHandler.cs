using FusionOS.Modules.Inventory.Application.Ledger.Contracts;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Ledger.Commands.AdjustStock;

public sealed class AdjustStockCommandHandler : IRequestHandler<AdjustStockCommand, InventoryLedgerEntryDto>
{
    private readonly IInventoryLedgerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AdjustStockCommandHandler(IInventoryLedgerRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<InventoryLedgerEntryDto> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        var entry = Domain.Ledger.InventoryLedgerEntry.RecordAdjustment(
            request.CompanyId, request.ProductId, request.WarehouseId, request.QuantityDelta, request.Reason, request.UnitCost, request.BatchNumber, request.SerialNumber);

        await _repository.AddAsync(entry, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new InventoryLedgerEntryDto(entry.Id, entry.ProductId, entry.WarehouseId, entry.QuantityDelta, entry.UnitCost, entry.BatchNumber, entry.SerialNumber, entry.Reason, entry.TransactionDate);
    }
}
