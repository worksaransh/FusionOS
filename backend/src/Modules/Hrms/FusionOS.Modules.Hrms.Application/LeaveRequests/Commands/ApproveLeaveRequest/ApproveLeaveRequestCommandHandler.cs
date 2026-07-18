using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.LeaveRequests.Commands.ApproveLeaveRequest;

public sealed class ApproveLeaveRequestCommandHandler : IRequestHandler<ApproveLeaveRequestCommand, LeaveRequestDto>
{
    private readonly ILeaveRequestRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveLeaveRequestCommandHandler(ILeaveRequestRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeaveRequestDto> Handle(ApproveLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await _repository.GetByIdAsync(request.CompanyId, request.LeaveRequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Leave request '{request.LeaveRequestId}' was not found.");

        leaveRequest.Approve();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LeaveRequestMapper.ToDto(leaveRequest);
    }
}
