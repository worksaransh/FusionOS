using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.Assets.Commands.UpdateAsset;

public sealed class UpdateAssetCommandHandler : IRequestHandler<UpdateAssetCommand, AssetDto>
{
    private readonly IAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAssetCommandHandler(IAssetRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AssetDto> Handle(UpdateAssetCommand request, CancellationToken cancellationToken)
    {
        var asset = await _repository.GetByIdAsync(request.CompanyId, request.AssetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Asset '{request.AssetId}' was not found.");

        asset.UpdateAssetDetails(request.Name, request.Location, request.WarrantyExpirationDate, request.WarrantyProvider);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AssetMapper.ToDto(asset);
    }
}
