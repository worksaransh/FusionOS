using FusionOS.Modules.Finance.Application.Receivables.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Receivables.Queries.GetCustomerBalance;

public sealed class GetCustomerBalanceQueryHandler : IRequestHandler<GetCustomerBalanceQuery, CustomerBalanceDto>
{
    private readonly IArLedgerRepository _repository;

    public GetCustomerBalanceQueryHandler(IArLedgerRepository repository) => _repository = repository;

    public async Task<CustomerBalanceDto> Handle(GetCustomerBalanceQuery request, CancellationToken cancellationToken)
    {
        var balance = await _repository.SumAmountAsync(request.CompanyId, request.CustomerId, cancellationToken);
        return new CustomerBalanceDto(request.CustomerId, balance);
    }
}
