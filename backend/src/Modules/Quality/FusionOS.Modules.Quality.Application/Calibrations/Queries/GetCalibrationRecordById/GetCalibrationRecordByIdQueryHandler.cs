using FusionOS.Modules.Quality.Application.Calibrations.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Calibrations.Queries.GetCalibrationRecordById;

public sealed class GetCalibrationRecordByIdQueryHandler : IRequestHandler<GetCalibrationRecordByIdQuery, CalibrationRecordDto>
{
    private readonly ICalibrationRecordRepository _repository;

    public GetCalibrationRecordByIdQueryHandler(ICalibrationRecordRepository repository) => _repository = repository;

    public async Task<CalibrationRecordDto> Handle(GetCalibrationRecordByIdQuery request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.CompanyId, request.CalibrationRecordId, cancellationToken)
            ?? throw new KeyNotFoundException($"Calibration record '{request.CalibrationRecordId}' was not found.");

        return CalibrationRecordMapper.ToDto(record);
    }
}
