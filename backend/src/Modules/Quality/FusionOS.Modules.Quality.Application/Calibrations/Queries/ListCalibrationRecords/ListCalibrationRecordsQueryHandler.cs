using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Calibrations.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.Calibrations.Queries.ListCalibrationRecords;

public sealed class ListCalibrationRecordsQueryHandler : IRequestHandler<ListCalibrationRecordsQuery, PagedResult<CalibrationRecordDto>>
{
    private readonly ICalibrationRecordRepository _repository;

    public ListCalibrationRecordsQueryHandler(ICalibrationRecordRepository repository) => _repository = repository;

    public async Task<PagedResult<CalibrationRecordDto>> Handle(ListCalibrationRecordsQuery request, CancellationToken cancellationToken)
    {
        var records = await _repository.ListAsync(request.CompanyId, request.OverdueOnly, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.OverdueOnly, cancellationToken);

        var dtos = records.Select(CalibrationRecordMapper.ToDto).ToList();

        return new PagedResult<CalibrationRecordDto>(dtos, request.Page, request.PageSize, total);
    }
}
