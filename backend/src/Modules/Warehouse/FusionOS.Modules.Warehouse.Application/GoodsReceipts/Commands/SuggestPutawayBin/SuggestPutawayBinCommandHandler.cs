using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.Bins.Contracts;
using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Commands.CreateGoodsReceipt;
using FusionOS.Modules.Warehouse.Application.GoodsReceipts.Contracts;
using FusionOS.Modules.Warehouse.Application.Warehouses.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.GoodsReceipts.Commands.SuggestPutawayBin;

public sealed class SuggestPutawayBinCommandHandler : IRequestHandler<SuggestPutawayBinCommand, GoodsReceiptDto>
{
    private readonly IGoodsReceiptRepository _repository;
    private readonly IBinRepository _binRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SuggestPutawayBinCommandHandler(IGoodsReceiptRepository repository, IBinRepository binRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _binRepository = binRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<GoodsReceiptDto> Handle(SuggestPutawayBinCommand request, CancellationToken cancellationToken)
    {
        var receipt = await _repository.GetByIdAsync(request.GoodsReceiptId, cancellationToken);
        if (receipt is null || receipt.CompanyId != request.CompanyId)
        {
            throw new KeyNotFoundException($"Goods receipt '{request.GoodsReceiptId}' was not found.");
        }

        var bin = await _binRepository.GetFirstActiveBinAsync(request.CompanyId, receipt.ZoneId, cancellationToken);
        if (bin is null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(receipt.ZoneId), "No active bin exists in this receipt's zone to suggest."),
            });
        }

        receipt.SuggestBin(request.LineId, bin.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateGoodsReceiptCommandHandler.MapToDto(receipt);
    }
}
