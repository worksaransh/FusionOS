using FusionOS.Modules.Quality.Application.Calibrations.Contracts;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Calibrations.Commands.CreateCalibrationRecord;

public sealed class CreateCalibrationRecordCommandHandler : IRequestHandler<CreateCalibrationRecordCommand, CalibrationRecordDto>
{
    private readonly ICalibrationRecordRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCalibrationRecordCommandHandler(ICalibrationRecordRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CalibrationRecordDto> Handle(CreateCalibrationRecordCommand request, CancellationToken cancellationToken)
    {
        var record = Domain.Calibrations.CalibrationRecord.Create(
            request.CompanyId,
            request.EquipmentReferenceId,
            request.EquipmentName,
            request.EquipmentIdentifier,
            request.CalibrationStandard,
            request.LastCalibratedDate,
            request.NextCalibrationDueDate,
            request.Result,
            request.PerformedByUserId,
            request.PerformedByExternalVendor,
            request.Notes);

        await _repository.AddAsync(record, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CalibrationRecordMapper.ToDto(record);
    }
}
