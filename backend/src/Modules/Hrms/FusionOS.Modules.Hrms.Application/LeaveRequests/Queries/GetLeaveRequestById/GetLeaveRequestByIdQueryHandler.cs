using FusionOS.Modules.Hrms.Application.LeaveRequests.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.LeaveRequests.Queries.GetLeaveRequestById;

public sealed class GetLeaveRequestByIdQueryHandler : IRequestHandler<GetLeaveRequestByIdQuery, LeaveRequestDto>
{
    private readonly ILeaveRequestRepository _repository;

    public GetLeaveRequestByIdQueryHandler(ILeaveRequestRepository repository) => _repository = repository;

    public async Task<LeaveRequestDto> Handle(GetLeaveRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var leaveRequest = await _repository.GetByIdAsync(request.CompanyId, request.LeaveRequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Leave request '{request.LeaveRequestId}' was not found.");

        return LeaveRequestMapper.ToDto(leaveRequest);
    }
}
