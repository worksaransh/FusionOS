using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.Assets.Queries.ListAssets;

public sealed class ListAssetsQueryHandler : IRequestHandler<ListAssetsQuery, PagedResult<AssetDto>>
{
    private readonly IAssetRepository _repository;

    public ListAssetsQueryHandler(IAssetRepository repository) => _repository = repository;

    public async Task<PagedResult<AssetDto>> Handle(ListAssetsQuery request, CancellationToken cancellationToken)
    {
        var assets = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = assets.Select(AssetMapper.ToDto).ToList();

        return new PagedResult<AssetDto>(dtos, request.Page, request.PageSize, total);
    }
}
