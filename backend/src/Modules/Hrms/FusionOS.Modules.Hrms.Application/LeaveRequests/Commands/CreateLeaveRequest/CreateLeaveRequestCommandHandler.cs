using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.LeaveRequests.Commands.CreateLeaveRequest;

/// <summary>Validates the Employee exists for this company before creating the request — same handler-level existence-check split CreateJournalEntryCommandHandler uses for JournalEntryLine.AccountId.</summary>
public sealed class CreateLeaveRequestCommandHandler : IRequestHandler<CreateLeaveRequestCommand, LeaveRequestDto>
{
    private readonly ILeaveRequestRepository _repository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateLeaveRequestCommandHandler(ILeaveRequestRepository repository, IEmployeeRepository employeeRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _employeeRepository = employeeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeaveRequestDto> Handle(CreateLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        if (!await _employeeRepository.ExistsAsync(request.CompanyId, request.EmployeeId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.EmployeeId), $"Employee '{request.EmployeeId}' does not exist for this company."),
            });
        }

        var leaveRequest = Domain.LeaveRequests.LeaveRequest.Create(
            request.CompanyId, request.EmployeeId, request.Type, request.StartDate, request.EndDate, request.Reason);

        await _repository.AddAsync(leaveRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LeaveRequestMapper.ToDto(leaveRequest);
    }
}
