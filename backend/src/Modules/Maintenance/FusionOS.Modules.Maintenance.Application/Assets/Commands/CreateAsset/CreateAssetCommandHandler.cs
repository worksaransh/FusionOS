using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.Assets.Commands.CreateAsset;

public sealed class CreateAssetCommandHandler : IRequestHandler<CreateAssetCommand, AssetDto>
{
    private readonly IAssetRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAssetCommandHandler(IAssetRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AssetDto> Handle(CreateAssetCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.CodeExistsAsync(request.CompanyId, request.Code, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Code), $"Asset code '{request.Code}' already exists for this company."),
            });
        }

        var asset = Domain.Assets.Asset.Create(request.CompanyId, request.Code, request.Name, request.Location);

        await _repository.AddAsync(asset, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AssetMapper.ToDto(asset);
    }
}
