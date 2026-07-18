using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using FusionOS.Modules.Finance.Application.FixedAssets.Commands.CreateFixedAsset;
using FusionOS.Modules.Finance.Application.FixedAssets.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Commands.UpdateFixedAsset;

public sealed class UpdateFixedAssetCommandHandler : IRequestHandler<UpdateFixedAssetCommand, FixedAssetDto>
{
    private readonly IFixedAssetRepository _repository;
    private readonly ICostCenterRepository _costCenterRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFixedAssetCommandHandler(IFixedAssetRepository repository, ICostCenterRepository costCenterRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _costCenterRepository = costCenterRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<FixedAssetDto> Handle(UpdateFixedAssetCommand request, CancellationToken cancellationToken)
    {
        var fixedAsset = await _repository.GetByIdAsync(request.CompanyId, request.FixedAssetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Fixed asset '{request.FixedAssetId}' was not found.");

        if (request.CostCenterId.HasValue &&
            await _costCenterRepository.GetByIdAsync(request.CompanyId, request.CostCenterId.Value, cancellationToken) is null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.CostCenterId), $"Cost center '{request.CostCenterId}' does not exist for this company."),
            });
        }

        fixedAsset.UpdateDetails(request.Name, request.CostCenterId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateFixedAssetCommandHandler.MapToDto(fixedAsset);
    }
}
