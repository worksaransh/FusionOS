using FusionOS.Modules.Hrms.Application.Attendance.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.Attendance.Queries.GetAttendanceRecordById;

public sealed class GetAttendanceRecordByIdQueryHandler : IRequestHandler<GetAttendanceRecordByIdQuery, AttendanceRecordDto>
{
    private readonly IAttendanceRecordRepository _repository;

    public GetAttendanceRecordByIdQueryHandler(IAttendanceRecordRepository repository) => _repository = repository;

    public async Task<AttendanceRecordDto> Handle(GetAttendanceRecordByIdQuery request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.CompanyId, request.AttendanceRecordId, cancellationToken)
            ?? throw new KeyNotFoundException($"Attendance record '{request.AttendanceRecordId}' was not found.");

        return AttendanceRecordMapper.ToDto(record);
    }
}
