using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.FixedAssets.Commands.CreateFixedAsset;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Queries.ListFixedAssets;

public sealed class ListFixedAssetsQueryHandler : IRequestHandler<ListFixedAssetsQuery, PagedResult<FixedAssetDto>>
{
    private readonly IFixedAssetRepository _repository;

    public ListFixedAssetsQueryHandler(IFixedAssetRepository repository) => _repository = repository;

    public async Task<PagedResult<FixedAssetDto>> Handle(ListFixedAssetsQuery request, CancellationToken cancellationToken)
    {
        var fixedAssets = await _repository.ListAsync(request.CompanyId, request.IsDisposed, request.IsActive, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.IsDisposed, request.IsActive, cancellationToken);

        var dtos = fixedAssets.Select(CreateFixedAssetCommandHandler.MapToDto).ToList();

        return new PagedResult<FixedAssetDto>(dtos, request.Page, request.PageSize, total);
    }
}
