using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.FixedAssets.Commands.CreateFixedAsset;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Commands.DisposeFixedAsset;

public sealed class DisposeFixedAssetCommandHandler : IRequestHandler<DisposeFixedAssetCommand, FixedAssetDto>
{
    private readonly IFixedAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DisposeFixedAssetCommandHandler(IFixedAssetRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FixedAssetDto> Handle(DisposeFixedAssetCommand request, CancellationToken cancellationToken)
    {
        var fixedAsset = await _repository.GetByIdAsync(request.CompanyId, request.FixedAssetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Fixed asset '{request.FixedAssetId}' was not found.");

        fixedAsset.Dispose(request.DisposedDate);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateFixedAssetCommandHandler.MapToDto(fixedAsset);
    }
}
