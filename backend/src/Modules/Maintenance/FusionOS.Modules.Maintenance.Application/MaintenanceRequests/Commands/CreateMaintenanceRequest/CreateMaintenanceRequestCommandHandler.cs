using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.CreateMaintenanceRequest;

/// <summary>Validates the Asset exists for this company before creating the request — same handler-level existence-check split CreateJournalEntryCommandHandler uses for JournalEntryLine.AccountId.</summary>
public sealed class CreateMaintenanceRequestCommandHandler : IRequestHandler<CreateMaintenanceRequestCommand, MaintenanceRequestDto>
{
    private readonly IMaintenanceRequestRepository _repository;
    private readonly IAssetRepository _assetRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMaintenanceRequestCommandHandler(IMaintenanceRequestRepository repository, IAssetRepository assetRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _assetRepository = assetRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MaintenanceRequestDto> Handle(CreateMaintenanceRequestCommand request, CancellationToken cancellationToken)
    {
        if (!await _assetRepository.ExistsAsync(request.CompanyId, request.AssetId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.AssetId), $"Asset '{request.AssetId}' does not exist for this company."),
            });
        }

        var maintenanceRequest = Domain.MaintenanceRequests.MaintenanceRequest.Create(request.CompanyId, request.AssetId, request.Type, request.Description);

        await _repository.AddAsync(maintenanceRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MaintenanceRequestMapper.ToDto(maintenanceRequest);
    }
}
