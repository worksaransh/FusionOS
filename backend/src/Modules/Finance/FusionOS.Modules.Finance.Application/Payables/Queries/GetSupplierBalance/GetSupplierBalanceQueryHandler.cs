using FusionOS.Modules.Finance.Application.Payables.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.Payables.Queries.GetSupplierBalance;

public sealed class GetSupplierBalanceQueryHandler : IRequestHandler<GetSupplierBalanceQuery, SupplierBalanceDto>
{
    private readonly IApLedgerRepository _repository;

    public GetSupplierBalanceQueryHandler(IApLedgerRepository repository) => _repository = repository;

    public async Task<SupplierBalanceDto> Handle(GetSupplierBalanceQuery request, CancellationToken cancellationToken)
    {
        var balance = await _repository.SumAmountAsync(request.CompanyId, request.SupplierId, cancellationToken);
        return new SupplierBalanceDto(request.SupplierId, balance);
    }
}
