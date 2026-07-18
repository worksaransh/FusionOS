using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.Assets.Queries.GetAssetById;

public sealed class GetAssetByIdQueryHandler : IRequestHandler<GetAssetByIdQuery, AssetDto>
{
    private readonly IAssetRepository _repository;

    public GetAssetByIdQueryHandler(IAssetRepository repository) => _repository = repository;

    public async Task<AssetDto> Handle(GetAssetByIdQuery request, CancellationToken cancellationToken)
    {
        var asset = await _repository.GetByIdAsync(request.CompanyId, request.AssetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Asset '{request.AssetId}' was not found.");

        return AssetMapper.ToDto(asset);
    }
}
