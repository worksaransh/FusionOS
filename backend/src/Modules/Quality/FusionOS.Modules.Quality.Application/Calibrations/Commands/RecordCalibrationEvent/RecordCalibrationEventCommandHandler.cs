using FusionOS.Modules.Quality.Application.Calibrations.Contracts;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Calibrations.Commands.RecordCalibrationEvent;

public sealed class RecordCalibrationEventCommandHandler : IRequestHandler<RecordCalibrationEventCommand, CalibrationRecordDto>
{
    private readonly ICalibrationRecordRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordCalibrationEventCommandHandler(ICalibrationRecordRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CalibrationRecordDto> Handle(RecordCalibrationEventCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.CompanyId, request.CalibrationRecordId, cancellationToken)
            ?? throw new KeyNotFoundException($"Calibration record '{request.CalibrationRecordId}' was not found.");

        record.RecordCalibration(
            request.CalibratedDate,
            request.NextDueDate,
            request.Result,
            request.PerformedByUserId,
            request.PerformedByExternalVendor,
            request.Notes);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CalibrationRecordMapper.ToDto(record);
    }
}
