using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.Assets.Commands.DeactivateAsset;

public sealed class DeactivateAssetCommandHandler : IRequestHandler<DeactivateAssetCommand, AssetDto>
{
    private readonly IAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateAssetCommandHandler(IAssetRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AssetDto> Handle(DeactivateAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await _repository.GetByIdAsync(request.CompanyId, request.AssetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Asset '{request.AssetId}' was not found.");

        asset.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AssetMapper.ToDto(asset);
    }
}
