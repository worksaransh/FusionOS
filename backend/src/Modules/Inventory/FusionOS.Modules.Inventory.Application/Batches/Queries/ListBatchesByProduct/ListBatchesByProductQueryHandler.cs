using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Batches.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Batches.Queries.ListBatchesByProduct;

public sealed class ListBatchesByProductQueryHandler : IRequestHandler<ListBatchesByProductQuery, PagedResult<BatchDto>>
{
    private readonly IBatchRepository _repository;

    public ListBatchesByProductQueryHandler(IBatchRepository repository) => _repository = repository;

    public async Task<PagedResult<BatchDto>> Handle(ListBatchesByProductQuery request, CancellationToken cancellationToken)
    {
        var batches = await _repository.ListByProductAsync(request.CompanyId, request.ProductId, request.ExpiringBefore, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountByProductAsync(request.CompanyId, request.ProductId, request.ExpiringBefore, cancellationToken);

        var dtos = batches.Select(BatchMapper.ToDto).ToList();

        return new PagedResult<BatchDto>(dtos, request.Page, request.PageSize, total);
    }
}
