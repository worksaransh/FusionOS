using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.LeaveRequests.Commands.RejectLeaveRequest;

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
