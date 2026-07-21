using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Hrms.Application.Attendance.Contracts;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.Attendance.Commands.UpdateAttendance;

public sealed class UpdateAttendanceCommandHandler : IRequestHandler<UpdateAttendanceCommand, AttendanceRecordDto>
{
    private readonly IAttendanceRecordRepository _repository;
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateAttendanceCommandHandler(
        IAttendanceRecordRepository repository,
        ILeaveRequestRepository leaveRequestRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _leaveRequestRepository = leaveRequestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AttendanceRecordDto> Handle(UpdateAttendanceCommand request, CancellationToken cancellationToken)
    {
        var record = await _repository.GetByIdAsync(request.CompanyId, request.AttendanceRecordId, cancellationToken)
            ?? throw new KeyNotFoundException($"Attendance record '{request.AttendanceRecordId}' was not found.");

        if (request.LeaveRequestId is { } leaveRequestId
            && await _leaveRequestRepository.GetByIdAsync(request.CompanyId, leaveRequestId, cancellationToken) is null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.LeaveRequestId), $"Leave request '{leaveRequestId}' does not exist for this company."),
            });
        }

        record.Update(request.CheckInTime, request.CheckOutTime, request.Status, request.LeaveRequestId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AttendanceRecordMapper.ToDto(record);
    }
}
