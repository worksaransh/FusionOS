using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.LeaveRequests.Commands.RejectLeaveRequest;

/// <summary>
/// No LeaveBalance restoration needed here: LeaveRequest.Reject() (see LeaveRequest.cs)
/// requires Status == Requested, the same one-clear-starting-state discipline Approve()
/// uses — a request can never be Approved then later Rejected in this state machine, so a
/// balance deduction (which only happens on Approve, in ApproveLeaveRequestCommandHandler)
/// can never have happened yet by the time Reject runs. There is currently no "reverse an
/// already-approved request" transition anywhere in this module; if one is added later, that
/// handler — not this one — would be the place to call LeaveBalance.Restore().
/// </summary>
public sealed class RejectLeaveRequestCommandHandler : IRequestHandler<RejectLeaveRequestCommand, LeaveRequestDto>
{
    private readonly ILeaveRequestRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RejectLeaveRequestCommandHandler(ILeaveRequestRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeaveRequestDto> Handle(RejectLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await _repository.GetByIdAsync(request.CompanyId, request.LeaveRequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Leave request '{request.LeaveRequestId}' was not found.");

        leaveRequest.Reject();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LeaveRequestMapper.ToDto(leaveRequest);
    }
}
