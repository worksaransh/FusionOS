using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Transfers.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Transfers.Queries.ListTransfers;

public sealed class ListTransfersQueryHandler : IRequestHandler<ListTransfersQuery, PagedResult<TransferDto>>
{
    private readonly ITransferRepository _repository;

    public ListTransfersQueryHandler(ITransferRepository repository) => _repository = repository;

    public async Task<PagedResult<TransferDto>> Handle(ListTransfersQuery request, CancellationToken cancellationToken)
    {
        var transfers = await _repository.ListAsync(request.CompanyId, request.ProductId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.ProductId, cancellationToken);

        var dtos = transfers.Select(TransferMapper.ToDto).ToList();

        return new PagedResult<TransferDto>(dtos, request.Page, request.PageSize, total);
    }
}
