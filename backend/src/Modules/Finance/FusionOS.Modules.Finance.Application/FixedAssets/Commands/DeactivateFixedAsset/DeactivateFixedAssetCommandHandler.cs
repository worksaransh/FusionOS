using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.FixedAssets.Commands.CreateFixedAsset;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Commands.DeactivateFixedAsset;

public sealed class DeactivateFixedAssetCommandHandler : IRequestHandler<DeactivateFixedAssetCommand, FixedAssetDto>
{
    private readonly IFixedAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateFixedAssetCommandHandler(IFixedAssetRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FixedAssetDto> Handle(DeactivateFixedAssetCommand request, CancellationToken cancellationToken)
    {
        var fixedAsset = await _repository.GetByIdAsync(request.CompanyId, request.FixedAssetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Fixed asset '{request.FixedAssetId}' was not found.");

        fixedAsset.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateFixedAssetCommandHandler.MapToDto(fixedAsset);
    }
}
