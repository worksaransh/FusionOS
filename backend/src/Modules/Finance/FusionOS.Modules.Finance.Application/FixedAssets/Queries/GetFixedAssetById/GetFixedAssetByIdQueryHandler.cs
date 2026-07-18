using FusionOS.Modules.Finance.Application.FixedAssets.Commands.CreateFixedAsset;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Queries.GetFixedAssetById;

public sealed class GetFixedAssetByIdQueryHandler : IRequestHandler<GetFixedAssetByIdQuery, FixedAssetDto>
{
    private readonly IFixedAssetRepository _repository;

    public GetFixedAssetByIdQueryHandler(IFixedAssetRepository repository) => _repository = repository;

    public async Task<FixedAssetDto> Handle(GetFixedAssetByIdQuery request, CancellationToken cancellationToken)
    {
        var fixedAsset = await _repository.GetByIdAsync(request.CompanyId, request.FixedAssetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Fixed asset '{request.FixedAssetId}' was not found.");

        return CreateFixedAssetCommandHandler.MapToDto(fixedAsset);
    }
}
