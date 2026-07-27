using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Maintenance.Application.Assets.Contracts;
using FusionOS.Modules.Maintenance.Application.MeterReadings.Contracts;
using MediatR;

namespace FusionOS.Modules.Maintenance.Application.MeterReadings.Commands.RecordMeterReading;

/// <summary>
/// Validates the Asset exists for this company before recording the reading —
/// same handler-level existence-check split CreateMaintenanceRequestCommandHandler
/// uses for AssetId.
///
/// Also enforces that readings are monotonically increasing per asset: a real
/// meter (hours run, odometer, cycle count) only counts up in normal operation, so
/// a new reading lower than the asset's latest recorded one is almost always a
/// data-entry mistake, not a legitimate new data point. A genuine meter
/// rollback/replacement is rare and exceptional enough that it should not be
/// accepted silently — it throws a ValidationException instead of quietly
/// recording a value that would corrupt any trend derived from these readings.
/// This is a new business rule introduced by this slice (nothing else in
/// Maintenance validates against prior state this way); the check is cheap — one
/// extra query for the asset's latest reading via IMeterReadingRepository.
/// GetLatestForAssetAsync — so it's done inline rather than deferred.
/// </summary>
public sealed class RecordMeterReadingCommandHandler : IRequestHandler<RecordMeterReadingCommand, MeterReadingDto>
{
    private readonly IMeterReadingRepository _repository;
    private readonly IAssetRepository _assetRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordMeterReadingCommandHandler(IMeterReadingRepository repository, IAssetRepository assetRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _assetRepository = assetRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MeterReadingDto> Handle(RecordMeterReadingCommand request, CancellationToken cancellationToken)
    {
        if (!await _assetRepository.ExistsAsync(request.CompanyId, request.AssetId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.AssetId), $"Asset '{request.AssetId}' does not exist for this company."),
            });
        }

        var latest = await _repository.GetLatestForAssetAsync(request.CompanyId, request.AssetId, cancellationToken);
        if (latest is not null && request.ReadingValue < latest.ReadingValue)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(
                    nameof(request.ReadingValue),
                    $"Reading value {request.ReadingValue} is less than this asset's latest recorded reading ({latest.ReadingValue} {latest.Unit} on {latest.ReadingDate:yyyy-MM-dd}). Meters are expected to only increase; if this is a genuine meter reset or replacement, confirm the value before resubmitting."),
            });
        }

        var reading = Domain.MeterReadings.MeterReading.Create(request.CompanyId, request.AssetId, request.ReadingValue, request.ReadingDate, request.Unit);

        await _repository.AddAsync(reading, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MeterReadingMapper.ToDto(reading);
    }
}
