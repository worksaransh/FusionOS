using FusionOS.Modules.Hrms.Application.LeaveBalances.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.LeaveBalances.Queries.ListLeaveBalancesForEmployee;

public sealed class ListLeaveBalancesForEmployeeQueryHandler : IRequestHandler<ListLeaveBalancesForEmployeeQuery, IReadOnlyList<LeaveBalanceDto>>
{
    private readonly ILeaveBalanceRepository _repository;

    public ListLeaveBalancesForEmployeeQueryHandler(ILeaveBalanceRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<LeaveBalanceDto>> Handle(ListLeaveBalancesForEmployeeQuery request, CancellationToken cancellationToken)
    {
        var balances = await _repository.ListForEmployeeAsync(request.CompanyId, request.EmployeeId, cancellationToken);
        return balances.Select(LeaveBalanceMapper.ToDto).ToList();
    }
}
