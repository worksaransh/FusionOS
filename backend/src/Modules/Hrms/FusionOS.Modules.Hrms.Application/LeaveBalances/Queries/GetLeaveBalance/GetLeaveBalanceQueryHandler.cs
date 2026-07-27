using FusionOS.Modules.Hrms.Application.LeaveBalances.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.LeaveBalances.Queries.GetLeaveBalance;

public sealed class GetLeaveBalanceQueryHandler : IRequestHandler<GetLeaveBalanceQuery, LeaveBalanceDto>
{
    private readonly ILeaveBalanceRepository _repository;

    public GetLeaveBalanceQueryHandler(ILeaveBalanceRepository repository) => _repository = repository;

    public async Task<LeaveBalanceDto> Handle(GetLeaveBalanceQuery request, CancellationToken cancellationToken)
    {
        var balance = await _repository.GetAsync(request.CompanyId, request.EmployeeId, request.LeaveType, request.Year, cancellationToken)
            ?? throw new KeyNotFoundException($"No leave balance found for employee '{request.EmployeeId}', type '{request.LeaveType}', year {request.Year}.");

        return LeaveBalanceMapper.ToDto(balance);
    }
}
