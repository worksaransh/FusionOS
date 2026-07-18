using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Bins.Contracts;
using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Commands.CreateGoodsReceipt;
using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.GoodsReceipts.Commands.ConfirmPutaway;

public sealed class ConfirmPutawayCommandHandler : IRequestHandler<ConfirmPutawayCommand, GoodsReceiptDto>
{
    private readonly IGoodsReceiptRepository _repository;
    private readonly IBinRepository _binRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmPutawayCommandHandler(IGoodsReceiptRepository repository, IBinRepository binRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _binRepository = binRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<GoodsReceiptDto> Handle(ConfirmPutawayCommand request, CancellationToken cancellationToken)
    {
        var receipt = await _repository.GetByIdAsync(request.GoodsReceiptId, cancellationToken);
        if (receipt is null || receipt.CompanyId != request.CompanyId)
        {
            throw new KeyNotFoundException($"Goods receipt '{request.GoodsReceiptId}' was not found.");
        }

        // Unlike PickList's BinId check (company-scoped only, since that handler
        // doesn't know a line's zone ahead of time), Putaway *does* know the
        // receipt's own Zone up front, so it can — and does — enforce that the
        // confirmed bin actually belongs to that same Zone, not just the company.
        var bin = await _binRepository.GetByIdAsync(request.BinId, cancellationToken);
        if (bin is null || bin.CompanyId != request.CompanyId || bin.ZoneId != receipt.ZoneId)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.BinId), "Bin does not exist in this receipt's zone."),
            });
        }

        receipt.ConfirmPutaway(request.LineId, bin.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateGoodsReceiptCommandHandler.MapToDto(receipt);
    }
}
