using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Hrms.Application.Attendance.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.Attendance.Queries.ListAttendanceRecords;

public sealed class ListAttendanceRecordsQueryHandler : IRequestHandler<ListAttendanceRecordsQuery, PagedResult<AttendanceRecordDto>>
{
    private readonly IAttendanceRecordRepository _repository;

    public ListAttendanceRecordsQueryHandler(IAttendanceRecordRepository repository) => _repository = repository;

    public async Task<PagedResult<AttendanceRecordDto>> Handle(ListAttendanceRecordsQuery request, CancellationToken cancellationToken)
    {
        var records = await _repository.ListAsync(request.CompanyId, request.EmployeeId, request.StartDate, request.EndDate, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.EmployeeId, request.StartDate, request.EndDate, cancellationToken);

        var dtos = records.Select(AttendanceRecordMapper.ToDto).ToList();

        return new PagedResult<AttendanceRecordDto>(dtos, request.Page, request.PageSize, total);
    }
}
