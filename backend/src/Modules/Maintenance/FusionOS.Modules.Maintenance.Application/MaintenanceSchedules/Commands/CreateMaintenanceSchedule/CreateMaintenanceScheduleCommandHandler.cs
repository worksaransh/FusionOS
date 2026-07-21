using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.MaintenanceSchedules.Commands.CreateMaintenanceSchedule;

/// <summary>Validates the Asset exists for this company before creating the schedule — same handler-level existence-check split CreateMaintenanceRequestCommandHandler uses for AssetId.</summary>
public sealed class CreateMaintenanceScheduleCommandHandler : IRequestHandler<CreateMaintenanceScheduleCommand, MaintenanceScheduleDto>
{
    private readonly IMaintenanceScheduleRepository _repository;
    private readonly IAssetRepository _assetRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMaintenanceScheduleCommandHandler(IMaintenanceScheduleRepository repository, IAssetRepository assetRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _assetRepository = assetRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MaintenanceScheduleDto> Handle(CreateMaintenanceScheduleCommand request, CancellationToken cancellationToken)
    {
        if (!await _assetRepository.ExistsAsync(request.CompanyId, request.AssetId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.AssetId), $"Asset '{request.AssetId}' does not exist for this company."),
            });
        }

        var schedule = Domain.MaintenanceSchedules.MaintenanceSchedule.Create(request.CompanyId, request.AssetId, request.Frequency, request.Description, request.NextDueDate);

        await _repository.AddAsync(schedule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MaintenanceScheduleMapper.ToDto(schedule);
    }
}
