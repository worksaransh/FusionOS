using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.PriceLists.Commands.CreatePriceList;
using FusionOS.Modules.Sales.Application.PriceLists.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.PriceLists.Queries.ListPriceLists;

public sealed class ListPriceListsQueryHandler : IRequestHandler<ListPriceListsQuery, PagedResult<PriceListDto>>
{
    private readonly IPriceListRepository _repository;

    public ListPriceListsQueryHandler(IPriceListRepository repository) => _repository = repository;

    public async Task<PagedResult<PriceListDto>> Handle(ListPriceListsQuery request, CancellationToken cancellationToken)
    {
        var priceLists = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        var dtos = priceLists.Select(CreatePriceListCommandHandler.MapToDto).ToList();

        return new PagedResult<PriceListDto>(dtos, request.Page, request.PageSize, total);
    }
}
