using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.Assets.Queries.ListAssetsWithExpiringWarranty;

public sealed class ListAssetsWithExpiringWarrantyQueryHandler : IRequestHandler<ListAssetsWithExpiringWarrantyQuery, IReadOnlyList<AssetDto>>
{
    private readonly IAssetRepository _repository;

    public ListAssetsWithExpiringWarrantyQueryHandler(IAssetRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<AssetDto>> Handle(ListAssetsWithExpiringWarrantyQuery request, CancellationToken cancellationToken)
    {
        var assets = await _repository.ListWithExpiringWarrantyAsync(request.CompanyId, request.WithinDays, cancellationToken);
        return assets.Select(AssetMapper.ToDto).ToList();
    }
}
