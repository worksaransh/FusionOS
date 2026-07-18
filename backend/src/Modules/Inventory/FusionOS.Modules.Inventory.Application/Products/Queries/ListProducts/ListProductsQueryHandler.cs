using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Inventory.Application.Products.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.Products.Queries.ListProducts;

public sealed class ListProductsQueryHandler : IRequestHandler<ListProductsQuery, PagedResult<ProductDto>>
{
    private readonly IProductRepository _repository;

    public ListProductsQueryHandler(IProductRepository repository) => _repository = repository;

    public async Task<PagedResult<ProductDto>> Handle(ListProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = products
            .Select(ProductMapper.ToDto)
            .ToList();

        return new PagedResult<ProductDto>(dtos, request.Page, request.PageSize, total);
    }
}
