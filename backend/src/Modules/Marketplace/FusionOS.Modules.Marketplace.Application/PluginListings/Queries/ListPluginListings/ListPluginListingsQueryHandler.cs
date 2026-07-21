using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Marketplace.Application.PluginListings.Contracts;
using MediatR;

namespace FusionOS.Modules.Marketplace.Application.PluginListings.Queries.ListPluginListings;

public sealed class ListPluginListingsQueryHandler : IRequestHandler<ListPluginListingsQuery, PagedResult<PluginListingDto>>
{
    private readonly IPluginListingRepository _repository;

    public ListPluginListingsQueryHandler(IPluginListingRepository repository) => _repository = repository;

    public async Task<PagedResult<PluginListingDto>> Handle(ListPluginListingsQuery request, CancellationToken cancellationToken)
    {
        var listings = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = listings.Select(PluginListingMapper.ToDto).ToList();

        return new PagedResult<PluginListingDto>(dtos, request.Page, request.PageSize, total);
    }
}
