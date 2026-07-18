using FusionOS.Modules.Hrms.Application.LeaveRequests.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.LeaveRequests.Queries.ListLeaveRequests;

public sealed class ListLeaveRequestsQueryHandler : IRequestHandler<ListLeaveRequestsQuery, PagedResult<LeaveRequestDto>>
{
    private readonly ILeaveRequestRepository _repository;

    public ListLeaveRequestsQueryHandler(ILeaveRequestRepository repository) => _repository = repository;

    public async Task<PagedResult<LeaveRequestDto>> Handle(ListLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        var requests = await _repository.ListAsync(request.CompanyId, request.EmployeeId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.EmployeeId, cancellationToken);

        var dtos = requests.Select(LeaveRequestMapper.ToDto).ToList();

        return new PagedResult<LeaveRequestDto>(dtos, request.Page, request.PageSize, total);
    }
}
