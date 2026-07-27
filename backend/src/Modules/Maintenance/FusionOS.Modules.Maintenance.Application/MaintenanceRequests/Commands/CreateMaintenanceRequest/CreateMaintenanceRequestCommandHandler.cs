using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using FusionOS.Modules.Maintenance.Application.ChecklistTemplates.Contracts;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Commands.CreateMaintenanceRequest;

/// <summary>
/// Validates the Asset exists for this company before creating the request — same handler-level
/// existence-check split CreateJournalEntryCommandHandler uses for JournalEntryLine.AccountId.
/// When ChecklistTemplateId is supplied (2026-07-23 follow-up), also loads that
/// ChecklistTemplate and snapshots its step descriptions onto the new request — see
/// MaintenanceRequest's own doc comment for why this is a value snapshot, not a live reference.
/// </summary>
public sealed class CreateMaintenanceRequestCommandHandler : IRequestHandler<CreateMaintenanceRequestCommand, MaintenanceRequestDto>
{
    private readonly IMaintenanceRequestRepository _repository;
    private readonly IAssetRepository _assetRepository;
    private readonly IChecklistTemplateRepository _checklistTemplateRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMaintenanceRequestCommandHandler(
        IMaintenanceRequestRepository repository,
        IAssetRepository assetRepository,
        IChecklistTemplateRepository checklistTemplateRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _assetRepository = assetRepository;
        _checklistTemplateRepository = checklistTemplateRepository;
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

        IReadOnlyCollection<string>? checklistItemDescriptions = null;
        if (request.ChecklistTemplateId.HasValue)
        {
            var template = await _checklistTemplateRepository.GetByIdAsync(request.CompanyId, request.ChecklistTemplateId.Value, cancellationToken)
                ?? throw new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(nameof(request.ChecklistTemplateId), $"Checklist template '{request.ChecklistTemplateId}' does not exist for this company."),
                });

            checklistItemDescriptions = template.Items.Select(i => i.Description).ToList();
        }

        var maintenanceRequest = Domain.MaintenanceRequests.MaintenanceRequest.Create(request.CompanyId, request.AssetId, request.Type, request.Description, checklistItemDescriptions);

        await _repository.AddAsync(maintenanceRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MaintenanceRequestMapper.ToDto(maintenanceRequest);
    }
}
