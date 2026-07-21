using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Hrms.Application.Attendance.Contracts;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.Attendance.Commands.RecordAttendance;

/// <summary>Validates the Employee exists (and, when supplied, the LeaveRequest) for this company before creating the record — same handler-level existence-check split CreateLeaveRequestCommandHandler uses for its own EmployeeId.</summary>
public sealed class RecordAttendanceCommandHandler : IRequestHandler<RecordAttendanceCommand, AttendanceRecordDto>
{
    private readonly IAttendanceRecordRepository _repository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveRequestRepository _leaveRequestRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RecordAttendanceCommandHandler(
        IAttendanceRecordRepository repository,
        IEmployeeRepository employeeRepository,
        ILeaveRequestRepository leaveRequestRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _employeeRepository = employeeRepository;
        _leaveRequestRepository = leaveRequestRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AttendanceRecordDto> Handle(RecordAttendanceCommand request, CancellationToken cancellationToken)
    {
        if (!await _employeeRepository.ExistsAsync(request.CompanyId, request.EmployeeId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.EmployeeId), $"Employee '{request.EmployeeId}' does not exist for this company."),
            });
        }

        if (request.LeaveRequestId is { } leaveRequestId
            && await _leaveRequestRepository.GetByIdAsync(request.CompanyId, leaveRequestId, cancellationToken) is null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.LeaveRequestId), $"Leave request '{leaveRequestId}' does not exist for this company."),
            });
        }

        var record = Domain.Attendance.AttendanceRecord.Create(
            request.CompanyId, request.EmployeeId, request.Date, request.CheckInTime, request.CheckOutTime, request.Status, request.LeaveRequestId);

        await _repository.AddAsync(record, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AttendanceRecordMapper.ToDto(record);
    }
}
