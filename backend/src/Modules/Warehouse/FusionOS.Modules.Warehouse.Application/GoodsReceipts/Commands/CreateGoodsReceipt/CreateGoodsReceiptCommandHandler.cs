using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.GoodsReceipts.Commands.CreateGoodsReceipt;

public sealed class CreateGoodsReceiptCommandHandler : IRequestHandler<CreateGoodsReceiptCommand, GoodsReceiptDto>
{
    private readonly IGoodsReceiptRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateGoodsReceiptCommandHandler(IGoodsReceiptRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<GoodsReceiptDto> Handle(CreateGoodsReceiptCommand request, CancellationToken cancellationToken)
    {
        if (!await _repository.ZoneExistsAsync(request.CompanyId, request.WarehouseId, request.ZoneId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.ZoneId), "Zone does not exist for this warehouse."),
            });
        }

        var receipt = Domain.GoodsReceipts.GoodsReceipt.Create(
            request.CompanyId,
            request.WarehouseId,
            request.ZoneId,
            request.PurchaseOrderId,
            request.SupplierId,
            request.Lines);

        await _repository.AddAsync(receipt, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(receipt);
    }

    internal static GoodsReceiptDto MapToDto(Domain.GoodsReceipts.GoodsReceipt receipt) => new(
        receipt.Id,
        receipt.WarehouseId,
        receipt.ZoneId,
        receipt.PurchaseOrderId,
        receipt.SupplierId,
        receipt.ReceivedDate,
        receipt.Lines.Select(l => new GoodsReceiptLineDto(l.Id, l.ProductId, l.QuantityReceived, l.UnitCost, l.BatchNumber, l.SerialNumber, l.SuggestedBinId, l.PutAwayBinId, l.IsPutAway)).ToList());
}
